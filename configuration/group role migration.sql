use dev_olab;

DROP TABLE `security_roles` ;

ALTER TABLE `security_users` 
	CHANGE COLUMN `user_id` `user_id` INT(10) UNSIGNED NOT NULL ;
ALTER TABLE `system_questions` 
	CHANGE COLUMN `counter_id` `counter_id` INT(10) UNSIGNED ;
ALTER TABLE `grouprole_acls` 
	ADD COLUMN `group_id` INT(10) UNSIGNED NOT NULL AFTER `acl`,
	ADD COLUMN `role_id` INT(10) UNSIGNED NULL AFTER `group_id`,
	ADD COLUMN `acl2` BIT(3) NOT NULL DEFAULT b'0' AFTER `role_id`;    
    
CREATE TABLE `user_grouproles` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `iss` VARCHAR(45) NOT NULL DEFAULT 'olab',
  `user_id` int(10) unsigned NOT NULL,
  `role_id` INT(10) UNSIGNED NULL,
  `group_id` int(10) unsigned NOT NULL,  
  `role` VARCHAR(45) NOT NULL,  
  PRIMARY KEY (`id`),
  CONSTRAINT `user_grouproles_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `user_grouproles_ibfk_2` FOREIGN KEY (`group_id`) REFERENCES `groups` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

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
  
CREATE TABLE `roles` (
  `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `description` VARCHAR(100),
  `name` VARCHAR(100) NOT NULL,
  `is_system` TINYINT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=1 
DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

UPDATE users 
	SET `group` = "olab";
UPDATE users 
	SET `role` = replace(`role`, 'olab', '' ) where `role` like 'olab%';

ALTER TABLE `grouprole_acls` 
	CHANGE COLUMN `name` `role_name` VARCHAR(45) NOT NULL;

UPDATE `grouprole_acls` 
	SET `role_name` = replace(`role_name`, 'olab', '' ) where `role_name` like 'olab%';
    
    
INSERT INTO `roles` (`name`) VALUES ('importer');
INSERT into `roles` (`name`) 
	SELECT DISTINCT role from `users` order by role;
INSERT into `roles` (`name`) 
	SELECT DISTINCT role_name FROM `grouprole_acls` 
    WHERE role_name NOT IN ( SELECT DISTINCT name from `roles` ) order by role_name;
UPDATE `roles` SET is_system = 1;
    
UPDATE `grouprole_acls` 
    SET `role_id` = ( SELECT `id` FROM `roles` WHERE `name` = `role_name` );
    
INSERT INTO `groups` (`name`) VALUES ('olab');
INSERT INTO `groups` (`name`) VALUES ('anonymous');
INSERT INTO `groups` (`name`) VALUES ('external');
ALTER TABLE `groups` 
	ADD COLUMN `is_system` TINYINT NOT NULL DEFAULT 0 AFTER `name`;
UPDATE `groups` SET is_system = 1;
    
CREATE TABLE IF NOT EXISTS  `grouprole_acls` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `imageable_id` int(10) unsigned NOT NULL,
  `imageable_type` varchar(45) NOT NULL,
  `group_id` int(10) unsigned NOT NULL,
  `role_id` int(10) unsigned DEFAULT NULL,
  `acl2` bit(3) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`id`),
  KEY `ifk_gra_role_idx` (`role_id`),
  KEY `ifk_gra_group_idx` (`group_id`),
  CONSTRAINT `ifk_gra_group` FOREIGN KEY (`group_id`) REFERENCES `groups` (`id`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `ifk_gra_role` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'external' ), null, 5 );
    
INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'external' ), ( SELECT id FROM `roles` where name = 'author' ), 7 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps',  ( SELECT id FROM `groups` where name = 'external' ), ( SELECT id FROM `roles` where name = 'superuser' ), 7 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Import',  ( SELECT id FROM `groups` where name = 'external' ), ( SELECT id FROM `roles` where name = 'superuser' ), 7 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'external' ), ( SELECT id FROM `roles` where name = 'director' ), 7 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'external' ), ( SELECT id FROM `roles` where name = 'administrator' ), 7 );
    
INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'external' ), ( SELECT id FROM `roles` where name = 'learner' ), 5 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Nodes', ( SELECT id FROM `groups` where name = 'external' ), null, 1 );


INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'anonymous' ), null, 5 );
    
INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'anonymous' ), ( SELECT id FROM `roles` where name = 'author' ), 7 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps',  ( SELECT id FROM `groups` where name = 'anonymous' ), ( SELECT id FROM `roles` where name = 'superuser' ), 7 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Import',  ( SELECT id FROM `groups` where name = 'anonymous' ), ( SELECT id FROM `roles` where name = 'superuser' ), 7 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'anonymous' ), ( SELECT id FROM `roles` where name = 'director' ), 7 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'anonymous' ), ( SELECT id FROM `roles` where name = 'administrator' ), 7 );
    
INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'anonymous' ), ( SELECT id FROM `roles` where name = 'learner' ), 5 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Nodes', ( SELECT id FROM `groups` where name = 'anonymous' ), null, 1 );

    
INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'olab' ), null, 5 );
    
INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'olab' ), ( SELECT id FROM `roles` where name = 'author' ), 7 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps',  ( SELECT id FROM `groups` where name = 'olab' ), ( SELECT id FROM `roles` where name = 'superuser' ), 7 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Import',  ( SELECT id FROM `groups` where name = 'olab' ), ( SELECT id FROM `roles` where name = 'superuser' ), 7 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'olab' ), ( SELECT id FROM `roles` where name = 'director' ), 7 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'olab' ), ( SELECT id FROM `roles` where name = 'administrator' ), 7 );
    
INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Maps', ( SELECT id FROM `groups` where name = 'olab' ), ( SELECT id FROM `roles` where name = 'learner' ), 5 );

INSERT INTO `grouprole_acls` (`imageable_id`, `imageable_type`, `group_id`, `role_id`, `acl2` )
	VALUES ( 0, 'Nodes', ( SELECT id FROM `groups` where name = 'olab' ), null, 1 );

/* ????  */
INSERT INTO `user_grouproles` (`user_id`, `group_id`, `role`)
	SELECT id, (SELECT id from `groups` where name = 'olab'), role
	FROM users where role is not null;
UPDATE `user_grouproles` 
	SET `role_id` = ( SELECT id from `roles` WHERE `name` = `role` );
ALTER IGNORE TABLE `user_grouproles` 
	MODIFY `role_id` INT(10) UNSIGNED NOT NULL,
    DROP COLUMN `role`;
  
INSERT INTO map_groups (map_id, group_id ) 
 SELECT id, (SELECT id from `groups` WHERE name = 'olab' ) FROM `maps` WHERE is_template = 0;
 
DROP VIEW IF EXISTS `orphanedconstantsview`;
DROP VIEW IF EXISTS `orphanedquestionsview`;
DROP TABLE IF EXISTS  `map_nodes_im`;
DROP TABLE IF EXISTS  `map_nodes_tmp`;
DROP TABLE IF EXISTS `user_groups`;

ALTER TABLE `users` 
	DROP COLUMN `role`,
	DROP COLUMN `group`;

INSERT INTO `users` 
	(`username`, `email`, `password`, `salt`, `nickname`, 
     `language_id`, `type_id`, `visualEditorAutosaveTime`, `modeUI`, `is_lti`) 
    VALUES ('anonymous', 'anon@example.com', '', '', 'anonymous', '0', '0', '50000', 'easy', '0');

ALTER TABLE `user_responses` 
DROP FOREIGN KEY `user_responses_ibfk_2`;

-- ALTER TABLE `user_responses` 
-- DROP INDEX `session_id` ;

-- ALTER TABLE `user_grouproles` 
-- ADD INDEX `user_grouproles_ibfk_3_idx` (`role_id` ASC) VISIBLE;

-- ALTER TABLE `user_grouproles` 
-- DROP COLUMN `role`;

ALTER TABLE `user_grouproles` 
ADD CONSTRAINT `user_grouproles_ibfk_3`
  FOREIGN KEY (`role_id`)
  REFERENCES `roles` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
  
  
  /* ------ */
  
ALTER TABLE `grouprole_acls` 
  DROP COLUMN `role_name`;
  
ALTER TABLE `grouprole_acls` 
  ADD INDEX `ifk_gra_role_idx` (`role_id` ASC) VISIBLE,
  ADD INDEX `ifk_gra_group_idx` (`group_id` ASC) VISIBLE;

ALTER TABLE `grouprole_acls` 
  ADD CONSTRAINT `ifk_gra_role`
    FOREIGN KEY (`role_id`)
    REFERENCES `roles` (`id`)
    ON DELETE CASCADE
    ON UPDATE NO ACTION,
  ADD CONSTRAINT `ifk_gra_group`
    FOREIGN KEY (`group_id`)
    REFERENCES `groups` (`id`)
    ON DELETE CASCADE
    ON UPDATE NO ACTION;
    
ALTER TABLE `security_users` 
  RENAME TO  `user_acls` ;
  
ALTER TABLE `user_acls` 
  ADD COLUMN `acl2` BIT(3) NOT NULL DEFAULT b'0' AFTER `acl`;    
  
UPDATE `user_acls` SET `acl2` = `acl2` | 4 WHERE `acl` LIKE '%R%';
UPDATE `user_acls` SET `acl2` = `acl2` | 2 WHERE `acl` LIKE '%W%';
UPDATE `user_acls` SET `acl2` = `acl2` | 1 WHERE `acl` LIKE '%X%';
  