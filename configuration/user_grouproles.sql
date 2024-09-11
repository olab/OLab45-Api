SELECT
	ugr.id,
    CONCAT( u.username, " (", ugr.user_id, ")" ) as `user`,
    CONCAT( g.name, " (", ugr.group_id, ")" ) as `group`,
    CONCAT( r.name, " (", ugr.role_id, ")" ) as `role`
FROM 
	user_grouproles ugr, 
	users u,
    `groups` g,
    `roles` r    
WHERE 
	ugr.user_id = u.id AND
    ugr.group_id IS NOT NULL AND
    ugr.role_id IS NOT NULL AND    
    ugr.group_id = g.id AND
    ugr.role_id = r.id
ORDER BY user
    