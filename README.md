# OLab45-Api

OLab 4.5 backend REST API

## Install

The API service requires .NET 6+ and MySQL or MariaDB installed. The instructions below are provided for RHEL and RHEL-like systems such as CentOS Stream.

### Prerequisites

The following commands will install the mysql server/client, dotnet sdk, nginx, in addition to some packages needed for the build, and whitelist the http/https ports for `firewalld`.

```sh
# update packages
sudo dnf update

# install .net 6
sudo dnf install dotnet-sdk-6.0

# verify you have .net 6 installed
dotnet --version

# install mariadb server and client - https://mariadb.com/kb/en/yum/
sudo dnf install mariadb-server

# enable and start mariadb service
sudo systemctl enable mariadb
sudo systemctl start mariadb

# inital mysql server setup
sudo mysql_secure_installation

# install nginx
sudo dnf install nginx

# enable and start nginx service
sudo systemctl enable nginx
sudo systemctl start nginx

# enable http and https if using firewalld
sudo firewall-cmd --permanent --add-service=http --add-service=https
sudo firewall-cmd --reload

# install extra dependencies needed
sudo dnf install git wget tar
```

## Building the API service

You can start by creating a build folder:

```sh
sudo mkdir -p /opt/olab46/api
```

### Initializing a MySQL connection

To create a MySQL user and database for the app, open a MySQL shell using `sudo mysql` and run the following commands, after updating the connection credentials:

```sql
create database olab46;
create user 'olab46'@'localhost' identified by 'str0ngPas$wordH3re';
grant all privileges on olab46.* to 'olab46'@'localhost' with grant option;
flush privileges;
exit;
```

Test your credentials and make sure everything works: `mysql -u olab46 -p olab46`

### Creating an app user

It's recommended to use a non-sudo user to manage the app files. For this demo, we'll use `olab`:

```sh
# add system user for running the app
sudo adduser olab

# add user to root group - this is important for running the app via systemd
sudo usermod -aG root olab
```

### Setting up the build source

Switch to your app user:

```sh
sudo su olab
```

Clone the GitHub repositories:

```sh
# API
git clone https://github.com/olab/OLab45-Api.git ~/Api

# Common
git clone https://github.com/olab/OLab45-Common.git ~/Common
```

Next, create an `appsettings.json` file for the web Api Service:

```sh
vi ~/Api/WebApiService/appsettings.json
```

Paste-in the following contents without these comments after making changes to `AppSettings.Secret` (JWT secret) and `ConnectionStrings.DefaultDatabase` (MySQL credentials):

```json
{
  "https_port": 5001,
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Error",
      "Microsoft.EntityFrameworkCore": "Error",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AppSettings": {
    "Secret": "0000000000000000",
    "Issuer": "olab",
    "Audience": "https://www.olab.ca",
    "WebsitePublicFilesDirectory": "/opt/olab46/api/static/files",
    "DefaultImportDirectory": "/tmp",
    "SignalREndpoint": "/olab/turktalk"
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultDatabase": "server=localhost;uid=olab46;pwd=str0ngPas$wordH3re;database=olab46"
  }
}
```

### Creating a custom systemd service

Exit your current user session to switch back to your root session: `exit`

Then, create the `olab46api` service:

```sh
sudo vi /usr/lib/systemd/system/olab46api.service
```

Paste-in the following configurations and change the `User` directive to your system user if not using `olab`:

```ini
[Unit]  
Description=OLab 4.6 API  

[Service]  
WorkingDirectory=/opt/olab46/api/Release/net6.0
ExecStart=/opt/olab46/api/Release/net6.0/OLabWebAPI --urls http://localhost:5001
Restart=always  
RestartSec=10
# enter your system user here
User=olab 
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1"
Environment="DOTNET_ROOT=/root/.dotnet"
Environment=PATH=/root/.dotnet:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin

[Install]  
WantedBy=multi-user.target
```

Don't start or enable the service just yet.

### Building the API service

Make sure to run the following commands as root:

```sh
# cd to the Api files directory
cd /home/olab/Api

# built the app
bash build.sh

# optionally enable the service to start at boot
sudo systemctl enable olab46api
```

The above commands should build the app and enable the service so it runs on `http://localhost:5001`. Don't worry if you see `HTTP 500` errors, we still need to run the database migrations to make things work.

### Initial Database Migration

We need to run the initial migration and update the database for the app to work. Run the commands below as `root`:

```sh
# as the root user
# install ef package
dotnet tool install --global dotnet-ef

# change directory
cd /home/olab/Common/Data

# add initial migration and update the database - run as root
dotnet ef --startup-project /home/olab/Api/WebApiService/ migrations add Initial -c OLabDBContext
dotnet ef --startup-project /home/olab/Api/WebApiService/ database update -c OLabDBContext
```

### Creating a proxy server

The goal of this step is to set up a reverse proxy with SSL to serve our (so far private, unless *:5001 is exposed) olab API.

You're required to have a FQDN for this step. We'll use `tutorial.olab.ca` in this demo.

#### If your install loads virtual host configurations from directories:

Then you're not required to create `/etc/nginx/sites-enabled` and tweak `nginx.conf`. You should, however, delete any default sites to proceed.

#### If your install loads virtual host configurations from `nginx.conf`:

If your nginx install doesn't come with a `sites-available` and `sites-enabled` directories and loading configurations from there, then you'll need to create this folder for hosting virtual hosts configurations:

```sh
sudo mkdir -p /etc/nginx/sites-enabled
```

Edit `/etc/nginx/nginx.conf` and remove any `server { .. }` blocks, then add this line towards the end of the `http { .. }` block:

```
    include /etc/nginx/sites-enabled/*;
```
#### Setting up a virtual host

Let's begin by creating an `olab.conf` file:

```sh
sudo vi /etc/nginx/sites-enabled/olab.conf
```

Paste-in the following configurations:

```
server {
  listen 80;
  listen [::]:80;
  server_name tutorial.olab.ca;

  root /usr/share/nginx/olab;
  index index.html;
  
  location /olab { proxy_pass http://localhost:5001; }
  location /player { try_files $uri $uri/ /player/index.html; }
  location /designer { try_files $uri $uri/ /designer/index.html; }
}
```

Allow nginx to connect to the network:

```sh
sudo setsebool -P httpd_can_network_connect 1

# this may also be needed on CentOS / RHEL if nginx starts throwing 403s for the player and/or designer
sudo setenforce permissive
```

Let's make a folder in which we will host the build outputs:

```sh
mkdir -p /usr/share/nginx/olab
# add user to nginx group
sudo usermod -aG nginx olab
# change owner to olab
sudo chown -R olab:nginx /usr/share/nginx/olab
# give permissions for nginx group
sudo chmod g+w -R /usr/share/nginx/olab
```

Lastly, restart the nginx service:

```sh
sudo systemctl restart nginx
```

Now you should be able to see a default 404 or 403 page from the API service if you open your domain in your browser.

## Encrypting Traffic with SSL

This part covers setting up a free SSL certificate from LetsEncrypt CA and automating renewals with a cronjob.

As a root user, run the following commands:

```sh
# add EPEL
sudo dnf install https://dl.fedoraproject.org/pub/epel/epel-release-latest-8.noarch.rpm

# install the dependencies required
sudo dnf install certbot python3-certbot-nginx

# deploy certificate - follow through the cli prompt
sudo certbot --nginx
```

In the last command, `certbot` will automatically update our virtual host config to add SSL support and reload the nginx server.

#### Automating Renewals

Once you have a certificate installed, you may want to automate renewals as these certificates expire every 3 months.

To do so, add the following cron script via `sudo crontab -e`

```
# renew any expiring-soon certificates at midnight UTC on Mons and Thurs
0 0 * * 1,4 /usr/bin/certbot renew --post-hook "systemctl reload nginx"
```

#### Fin

That's it for building and deploying the API service. If you wish to continue with setting up the OLab Player and Designer applications in this server, then follow along with the rest of this article.

## Building the Player and Designer Applications

### Prerequisites

You will need `node` and `npm` to build the OLab Designer and Player applications. Let's install node 18 LTS:

```sh
# install node 18
curl -sL https://rpm.nodesource.com/setup_18.x | sudo bash -
sudo dnf install -y nodejs
```

Confirm you have node 18+ and npm installed

```sh
node -v && npm -v
```

### OLab Player

To install, build and deploy the Player application, switch to your system user and clone the source repository:

```sh
# switch user
su olab

# clone repo
git clone https://github.com/olab/OLab45-Player.git ~/Player
```

Install NPM modules and build the React.js application:

```sh
# cd to Player folder
cd ~/Player

# install packages and run build
npm install

# edit src/config.js and remove any hard-coded FQDNs from prod object properties
# e.g. from: API_URL: "https://logan.cardinalcreek.ca/olab/api/v3" to: API_URL: "/olab/api/v3"
# e.g. from: TTALK_HUB_URL: "https://logan.cardinalcreek.ca/turktalk" to: TTALK_HUB_URL: "/turktalk"
vi src/config.js

# build the react app
BUILD_PATH=/usr/share/nginx/olab/player npx react-scripts build
```

### OLab Designer

To install, build and deploy the Designer application, switch to your system user and clone the source repository:

```sh
# switch user if not done already
su olab

# clone repo
git clone https://github.com/olab/OLab45-Designer.git ~/Designer
```

Install NPM modules and build the React.js application:

```sh
# cd to Designer folder
cd ~/Designer

# create a .env file if not done already
test -f .env || cp .env.sample .env

# edit .env (vi .env) and set the following environment variable values 
API_URL=/olab/api/v3
PLAYER_PUBLIC_URL=/player
PUBLIC_URL=/designer

# install packages and run build
npm install && npm run build
```

The last step required is to copy the build artifacts over to the web server folder:

```sh
# copy the build folder to the webserver folder
rm /usr/share/nginx/olab/designer -rf
mv build /usr/share/nginx/olab/designer
```