SELECT 
	u.id,
    u.username,
    CONCAT( g.name, " (", g.id, ")" ) as `group`,
    CONCAT( r.name, " (", r.id, ")" ) as `role`    
FROM 
	`users` u, 
    `user_grouproles` ugr,
	`groups` g,
    `roles` r
WHERE
	u.id = ugr.user_id AND
    ugr.group_id = g.id AND
    ugr.role_id = r.id
ORDER BY username    