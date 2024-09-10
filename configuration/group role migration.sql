use dev_olab;

DROP TABLE `security_roles` ;

CREATE TABLE IF NOT EXISTS `roles` (
  `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `description` VARCHAR(100),
  `name` VARCHAR(100) NOT NULL,
  `is_system` TINYINT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=1 
DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `user_grouproles` (
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

UPDATE users 
	SET `group` = "olab";
UPDATE users 
	SET `role` = replace(`role`, 'olab', '' ) where `role` like 'olab%';
    
INSERT INTO `groups` (`name`) VALUES ('olab');
INSERT INTO `groups` (`name`) VALUES ('anonymous');
INSERT INTO `groups` (`name`) VALUES ('external');
ALTER TABLE `groups` 
	ADD COLUMN `is_system` TINYINT NOT NULL DEFAULT 0 AFTER `name`;
UPDATE `groups` SET is_system = 1;    

INSERT INTO `roles` (`name`, `is_system`) VALUES ('importer', 1);
INSERT INTO `roles` (`name`, `is_system`) VALUES ('moderator', 1);
INSERT into `roles` (`name`) 
	SELECT DISTINCT role from `users` order by role;
UPDATE `roles` SET is_system = 1;

ALTER TABLE `security_users` 
	CHANGE COLUMN `user_id` `user_id` INT(10) UNSIGNED NOT NULL ;
ALTER TABLE `system_questions` 
	CHANGE COLUMN `counter_id` `counter_id` INT(10) UNSIGNED ;    

/* ????  */
INSERT INTO `user_grouproles` (`user_id`, `group_id`, `role`)
	SELECT id, (SELECT id from `groups` where name = 'olab'), role
	FROM users where role is not null;
UPDATE `user_grouproles` 
	SET `role_id` = ( SELECT id from `roles` WHERE `name` = `role` );
ALTER IGNORE TABLE `user_grouproles` 
	MODIFY `role_id` INT(10) UNSIGNED NOT NULL,
    DROP COLUMN `role`;
 
DROP VIEW IF EXISTS `orphanedconstantsview`;
DROP VIEW IF EXISTS `orphanedquestionsview`;
DROP TABLE IF EXISTS  `map_nodes_im`;
DROP TABLE IF EXISTS  `map_nodes_tmp`;
DROP TABLE IF EXISTS `user_groups`;

ALTER TABLE `users` 
	DROP COLUMN `role`,
	DROP COLUMN `group`;

DELETE FROM `users` WHERE username LIKE 'anon%';

INSERT INTO `users` 
	(`username`, `email`, `password`, `salt`, `nickname`, 
     `language_id`, `type_id`, `visualEditorAutosaveTime`, `modeUI`, `is_lti`) 
    VALUES ('anonymous', 'anon@example.com', '', '', 'anonymous', '0', '0', '50000', 'easy', '0');

INSERT INTO `user_grouproles`
	( `iss`, `user_id`, `role_id`, `group_id`)
VALUES ( 
	'olab',  
    (SELECT id from `users` WHERE username = 'anonymous' ), 
    (SELECT id from `roles` WHERE name = 'learner' ), 
    (SELECT id from `groups` WHERE name = 'anonymous' ));

ALTER TABLE `user_responses` 
DROP FOREIGN KEY `user_responses_ibfk_2`;

ALTER TABLE `user_grouproles` 
ADD CONSTRAINT `user_grouproles_ibfk_3`
  FOREIGN KEY (`role_id`)
  REFERENCES `roles` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
    
-- clean up not-needed roles    
DELETE FROM `user_grouproles` WHERE `role_id` IN (2, 7, 8, 9, 11 );  
DELETE FROM `roles` WHERE `id` IN (2, 7, 8, 9, 11 );  

UPDATE `maps` SET created_at = NOW() WHERE created_at is NULL;

-- give everyone default olab:learner access
DELETE FROM `user_grouproles` WHERE 
	`role_id` = (SELECT id from `roles` WHERE name = 'learner') AND
    `group_id` = (SELECT id from `groups` WHERE name = 'olab');
INSERT INTO `user_grouproles` ( `iss`, `user_id`, `group_id`, `role_id` )
	SELECT 
		'olab', 
        id, 
        (SELECT id from `groups` WHERE name = 'olab'), 
        (SELECT id from `roles` WHERE name = 'learner') 
	FROM `users` WHERE username <> 'anonymous';

CREATE TABLE IF NOT EXISTS `system_applications` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `map_node_grouproles` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `node_id` int(10) unsigned NOT NULL,
  `group_id` int(10) unsigned DEFAULT NULL,
  `role_id` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `group_id` (`group_id`),
  KEY `role_id` (`role_id`),
  KEY `mngr_ibfk_node_idx` (`node_id`),
  CONSTRAINT `mngr_ibfk_group` FOREIGN KEY (`group_id`) REFERENCES `groups` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `mngr_ibfk_node` FOREIGN KEY (`node_id`) REFERENCES `map_nodes` (`id`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `mngr_ibfk_role` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `map_grouproles` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `map_id` int(10) unsigned NOT NULL,
  `group_id` int(10) unsigned DEFAULT NULL,
  `role_id` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `group_id` (`group_id`),
  KEY `role_id` (`role_id`),
  KEY `mgr_ibfk_node_idx` (`map_id`),
  CONSTRAINT `mgr_ibfk_group` FOREIGN KEY (`group_id`) REFERENCES `groups` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `mgr_ibfk_node` FOREIGN KEY (`map_id`) REFERENCES `maps` (`id`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `mgr_ibfk_role` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- add anonymous group to all anonymous maps
INSERT INTO `map_grouproles` (`map_id`, `group_id`, `role_id` )
	SELECT id, (SELECT id from `groups` WHERE name = 'anonymous' ), NULL FROM `maps` WHERE security_id = 1;  
    
-- add olab group to all anonymous maps
INSERT INTO `map_grouproles` (`map_id`, `group_id`, `role_id` )
	SELECT id, (SELECT id from `groups` WHERE name = 'olab' ), NULL FROM `maps`;