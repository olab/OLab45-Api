SELECT DISTINCT
    gra.id,
    IF( gra.group_id IS NOT NULL, ( SELECT CONCAT( name, " (", id, ")" ) from `groups` WHERE id = gra.group_id ), null ) as `group`,
    IF( gra.role_id IS NOT NULL, ( SELECT CONCAT( name, " (", id, ")" ) from `roles` WHERE id = gra.role_id ), null ) as `role`,
    gra.imageable_type as `type`, 
	IF( gra.imageable_id IS NOT NULL, CONCAT( sa.name, " (", sa.id, ")" ), null ) as `application`,    
    gra.acl2 
FROM 
	`grouprole_acls` gra,  
    `system_applications` sa
WHERE 
    gra.imageable_type = 'Apps' AND
    ( gra.imageable_id = sa.id OR gra.imageable_id is null)
ORDER BY
	`group`, `role`, `application`
    
    