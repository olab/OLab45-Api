use dev_olab;

START TRANSACTION;

CREATE TABLE `user_counter_update` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `counter_state` VARCHAR(8192) NOT NULL,
  PRIMARY KEY (`id`));

CREATE TABLE `usersessiontrace_counterupdate` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `sessiontrace_id` INT(10) UNSIGNED NOT NULL,
  `counterupdate_id` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY (`id`));

ALTER TABLE `usersessiontrace_counterupdate` 
ADD INDEX `stcu_fk_st_idx` (`sessiontrace_id` ASC) VISIBLE,
ADD INDEX `stcu_fk_cu_idx` (`counterupdate_id` ASC) VISIBLE;
;
ALTER TABLE `usersessiontrace_counterupdate` 
ADD CONSTRAINT `stcu_fk_st`
  FOREIGN KEY (`sessiontrace_id`)
  REFERENCES `user_sessiontraces` (`id`)
  ON DELETE CASCADE
  ON UPDATE NO ACTION,
ADD CONSTRAINT `stcu_fk_cu`
  FOREIGN KEY (`counterupdate_id`)
  REFERENCES `user_counter_update` (`id`)
  ON DELETE CASCADE
  ON UPDATE NO ACTION;

CREATE TABLE `userresponse_counterupdate` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `userresponse_id` INT(10) UNSIGNED NOT NULL,
  `counterupdate_id` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY (`id`));
  
ALTER TABLE `userresponse_counterupdate` 
ADD INDEX `urcu_fk_ur_idx` (`userresponse_id` ASC) VISIBLE,
ADD INDEX `urcu_fk_cu_idx` (`counterupdate_id` ASC) VISIBLE;
;
ALTER TABLE `userresponse_counterupdate` 
ADD CONSTRAINT `urcu_fk_ur`
  FOREIGN KEY (`userresponse_id`)
  REFERENCES `user_responses` (`id`)
  ON DELETE CASCADE
  ON UPDATE NO ACTION,
ADD CONSTRAINT `urcu_fk_cu`
  FOREIGN KEY (`counterupdate_id`)
  REFERENCES `user_counter_update` (`id`)
  ON DELETE CASCADE
  ON UPDATE NO ACTION;

DROP VIEW IF EXISTS `orphanedconstantsview`;
DROP VIEW IF EXISTS `orphanedquestionsview`;
DROP TABLE IF EXISTS  `map_nodes_im`, `map_nodes_tmp`;

ALTER TABLE `security_users` 
CHANGE COLUMN `user_id` `user_id` INT(10) UNSIGNED NOT NULL;

ALTER TABLE `system_questions` 
CHANGE COLUMN `counter_id` `counter_id` INT(10) UNSIGNED NULL DEFAULT NULL ;


COMMIT;