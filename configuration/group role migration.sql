use dev_olab;

START TRANSACTION;

CREATE TABLE `roles` (
  `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `description` VARCHAR(100),
  `name` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=1 
DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
INSERT INTO `roles` (`name`) VALUES ('importer');

ALTER TABLE `system_counter_actions` 
DROP FOREIGN KEY `fk_counter_action_counter`;
ALTER TABLE `system_counter_actions` 
ADD CONSTRAINT `fk_counter_action_counter`
  FOREIGN KEY (`counter_id`)
  REFERENCES `system_counters` (`id`)
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE `system_questions` 
CHANGE COLUMN `counter_id` `counter_id` INT(10) UNSIGNED ;

ALTER TABLE `security_users` 
CHANGE COLUMN `user_id` `user_id` INT(10) UNSIGNED NOT NULL,
ADD COLUMN `acl2` BIT(3) NOT NULL DEFAULT b'0' AFTER `acl`;

ALTER TABLE `security_roles` 
ADD COLUMN `group_id` INT(10) UNSIGNED NOT NULL AFTER `acl`,
ADD COLUMN `role_id` INT(10) UNSIGNED NOT NULL AFTER `group_id`,
ADD COLUMN `acl2` BIT(3) NOT NULL DEFAULT b'0' AFTER `role_id`;
  
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

UPDATE `users` SET `group` = "olab";
UPDATE `users` SET role = "moderator" WHERE role = "olabmod";
UPDATE `users` SET role = "learner" WHERE role = "olablearner";
UPDATE `users` SET role = "author" WHERE role = "olabauthor";
UPDATE `users` SET role = "guest" WHERE role = "olabguest";
UPDATE `users` SET role = "superuser" WHERE role = "olabsuperuser";
UPDATE `users` SET role = "learner" WHERE role = "";
UPDATE `users` SET role = "director" WHERE role = "olabdirector";
UPDATE `users` SET role = "administrator" WHERE role = "olabadministrator";

INSERT into `roles` (`name`) 
	SELECT DISTINCT role from `users` order by role;
INSERT into `roles` (`name`) 
	SELECT DISTINCT name FROM `security_roles` 
    WHERE name NOT IN ( SELECT DISTINCT name from `roles` ) order by name;

ALTER TABLE `users` 
	ADD COLUMN `role_id` INT(10) UNSIGNED;
UPDATE `users` 
	SET role_id = ( SELECT id from `roles` WHERE name = `users`.role);

ALTER TABLE `user_groups` 
	ADD COLUMN `role_id` INT(10) UNSIGNED NOT NULL AFTER `group_id`,
    ADD COLUMN `iss` VARCHAR(45) NOT NULL DEFAULT 'olab' AFTER `id`;
    
ALTER TABLE `user_groups` 
ADD CONSTRAINT `user_groups_ibfk_3`
  FOREIGN KEY (`role_id`)
  REFERENCES `roles` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
    
COMMIT;

-- *************************

ALTER TABLE `users` 
DROP COLUMN `role`,
DROP COLUMN `role_id`,
DROP COLUMN `group`;

SELECT @group_id := id FROM `groups` WHERE name = 'olab';
SELECT @role_id := id FROM `roles` WHERE name = 'learner';
SELECT @max_id := MAX(id) FROM `users`;
SET @batch_size = @max_id / 10;
SET @num_batches = @max_id / @batch_size;

INSERT INTO `user_groups` (user_id, group_id, role_id )
	SELECT id, @group_id, @role_id
	FROM `users`;

-- DELIMITER //
-- FOR l in 0..@num_batches
-- DO
--     START TRANSACTION;
--     SET @start = l * @batch_size;
--     SET @end = ( l + 1 ) * @batch_size;
--     INSERT INTO `user_groups` (user_id, group_id, role_id )
--         SELECT id, @group_id, @role_id
--         FROM `users` WHERE id >= @start and id <= @end;
--     COMMIT;
-- END FOR;
-- //

UPDATE `user_groups` SET role_id = ( SELECT role_id FROM `users` WHERE id = user_groups.user_id );  
  
UPDATE `security_users` set acl2 = 4 WHERE acl like '%R%';
UPDATE security_users set acl2 = acl2+2  WHERE acl like '%W%';
UPDATE security_users set acl2 = acl2+1  WHERE acl like '%X%';
  
UPDATE `security_roles` 
SET role_id = ( SELECT id from `roles` WHERE name = security_roles.name ); 
UPDATE security_roles set acl2 = 4 WHERE acl like '%R%';
UPDATE security_roles set acl2 = acl2+2  WHERE acl like '%W%';
UPDATE security_roles set acl2 = acl2+1  WHERE acl like '%X%';
  
ALTER TABLE `security_roles` 
DROP COLUMN `name`,
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

INSERT INTO `users` (`username`, `email`, `password`, `salt`, `nickname`, `language_id`, `type_id`, `visualEditorAutosaveTime`, `modeUI`, `is_lti`) VALUES 
('anonymous', 'anon@example.com', '', '', 'anonymous', '0', '0', '50000', 'easy', '0');
