use dev_olab;

CREATE TABLE `roles` (
  `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `description` VARCHAR(100),
  `name` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
INSERT INTO `roles` (`name`) VALUES ('importer');

ALTER TABLE `security_users` 
CHANGE COLUMN `user_id` `user_id` INT(10) UNSIGNED NOT NULL ;

ALTER TABLE `system_questions` 
CHANGE COLUMN `counter_id` `counter_id` INT(10) UNSIGNED ;

ALTER TABLE `security_roles` 
ADD COLUMN `group_id` INT(10) UNSIGNED NOT NULL AFTER `acl`,
ADD COLUMN `role_id` INT(10) UNSIGNED NOT NULL AFTER `group_id`;

UPDATE `security_roles` SET `group_id` = 1;
UPDATE `security_roles` SET name = "moderator" WHERE name = "olabmod";
UPDATE `security_roles` SET name = "learner" WHERE name = "olablearner";
UPDATE `security_roles` SET name = "author" WHERE name = "olabauthor";
UPDATE `security_roles` SET name = "guest" WHERE name = "olabguest";
UPDATE `security_roles` SET name = "superuser" WHERE name = "olabsuperuser";
UPDATE `security_roles` SET name = "administrator" WHERE name = "olabadministrator";

ALTER TABLE `groups` 
ADD COLUMN `description` VARCHAR(100) AFTER `id`;
INSERT INTO `groups` (`name`) VALUES ('olab');
INSERT INTO `groups` (`name`) VALUES ('anonymous');
INSERT INTO `groups` (`name`) VALUES ('external');

UPDATE users SET `group` = "olab";
UPDATE users SET role = "moderator" WHERE role = "olabmod";
UPDATE users SET role = "learner" WHERE role = "olablearner";
UPDATE users SET role = "author" WHERE role = "olabauthor";
UPDATE users SET role = "guest" WHERE role = "olabguest";
UPDATE users SET role = "superuser" WHERE role = "olabsuperuser";
UPDATE users SET role = "learner" WHERE role = "";
UPDATE `users` SET role = "administrator" WHERE role = "olabadministrator";

INSERT into `roles` (`name`) 
	SELECT DISTINCT role from `users`;
INSERT into `roles` (`name`) 
	SELECT DISTINCT name FROM `security_roles` 
    WHERE name NOT IN ( SELECT DISTINCT name from `roles` );

ALTER TABLE `users` 
	ADD COLUMN `role_id` INT(10) UNSIGNED;
UPDATE `users` 
	SET role_id = ( SELECT id from `roles` WHERE name = `users`.role);

ALTER TABLE `user_groups` 
	ADD COLUMN `role_id` INT(10) UNSIGNED NOT NULL AFTER `group_id`;

ALTER TABLE `user_groups` 
ADD CONSTRAINT `user_groups_ibfk_3`
  FOREIGN KEY (`role_id`)
  REFERENCES `roles` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
  
SELECT @group_id := id FROM `groups` WHERE name = 'olab';
SELECT @role_id := id FROM `roles` WHERE name = 'learner';
  
INSERT INTO `user_groups` (user_id, group_id, role_id )
SELECT id, @group_id, @role_id
FROM `users`;
  
UPDATE `user_groups` SET role_id = ( SELECT role_id FROM `users` WHERE id = user_groups.user_id );  
  
UPDATE `security_roles` 
SET role_id = ( SELECT id from `roles` WHERE name = security_roles.name );
ALTER TABLE `security_roles` 
DROP COLUMN `name`;

ALTER TABLE `security_roles` 
ADD CONSTRAINT `security_roles_ibfk_1`
  FOREIGN KEY (`group_id`)
  REFERENCES `groups` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `security_roles_ibfk_2`
  FOREIGN KEY (`role_id`)
  REFERENCES `roles` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
  
ALTER TABLE `map_groups` 
ADD CONSTRAINT `mp_ibfk_map`
  FOREIGN KEY (`map_id`)
  REFERENCES `maps` (`id`)
  ON DELETE CASCADE
  ON UPDATE NO ACTION,
ADD CONSTRAINT `mp_ibfk_group`
  FOREIGN KEY (`group_id`)
  REFERENCES `groups` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
  
INSERT INTO map_groups (map_id, group_id ) 
 SELECT id, (SELECT id from `groups` WHERE name = 'olab' ) FROM `maps` WHERE is_template = 0;

DROP VIEW IF EXISTS `orphanedconstantsview`;
DROP VIEW IF EXISTS `orphanedquestionsview`;
DROP TABLE IF EXISTS  `map_nodes_im`, `map_nodes_tmp`;

ALTER TABLE `users` 
DROP COLUMN `role`,
DROP COLUMN `role_id`,
DROP COLUMN `group`;

INSERT INTO `users` (`username`, `email`, `password`, `salt`, `nickname`, `language_id`, `type_id`, `visualEditorAutosaveTime`, `modeUI`, `is_lti`) VALUES 
('anonymous', 'anon@example.com', '', '', 'anonymous', '0', '0', '50000', 'easy', '0');
