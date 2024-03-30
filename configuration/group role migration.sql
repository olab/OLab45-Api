use olab_dev;

ALTER TABLE `security_users` 
CHANGE COLUMN `user_id` `user_id` INT(10) UNSIGNED NOT NULL ;

ALTER TABLE `system_questions` 
CHANGE COLUMN `counter_id` `counter_id` INT(10) UNSIGNED ;

update users set `group` = "olab";
update users set role = "moderator" where role = "olabmod";
update users set role = "learner" where role = "olablearner";
update users set role = "author" where role = "olabauthor";
update users set role = "guest" where role = "olabguest";
update users set role = "superuser" where role = "olabsuperuser";

ALTER TABLE `security_roles` 
ADD COLUMN `group_id` INT(10) UNSIGNED NOT NULL AFTER `acl`,
CHANGE COLUMN `name` `role` VARCHAR(45) NOT NULL AFTER `acl`;

ALTER TABLE `user_groups` 
ADD COLUMN `role` VARCHAR(45) NOT NULL AFTER `group_id`;

update security_Roles set `group_id` = 1;
update security_Roles set role = "moderator" where role = "olabmod";
update security_Roles set role = "learner" where role = "olablearner";
update security_Roles set role = "author" where role = "olabauthor";
update security_Roles set role = "guest" where role = "olabguest";
update security_Roles set role = "superuser" where role = "olabsuperuser";

INSERT INTO `groups` (`name`) VALUES ('olab');
INSERT INTO `groups` (`name`) VALUES ('anonymous');
INSERT INTO `groups` (`name`) VALUES ('external');

ALTER TABLE `user_groups` 
ADD COLUMN `iss` VARCHAR(45) NOT NULL DEFAULT 'olab' AFTER `id`;

INSERT INTO user_groups (user_id, group_id, `role`)
SELECT id, (SELECT id from `groups` where name = 'olab'), role
FROM users where role is not null;

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

DROP VIEW `orphanedconstantsview`;
DROP VIEW `orphanedquestionsview`;
DROP TABLE `map_nodes_im`, `map_nodes_tmp`;

ALTER TABLE `users` 
DROP COLUMN `role`,
DROP COLUMN `group`;

INSERT INTO `users` (`username`, `email`, `password`, `salt`, `nickname`, `language_id`, `type_id`, `visualEditorAutosaveTime`, `modeUI`, `is_lti`) VALUES 
('anonymous', 'anon@example.com', '', '', 'anonymous', '0', '0', '50000', 'easy', '0');



