SELECT gra.id, g.name as group_name, r.name as role_name, gra.imageable_type, gra.imageable_id, gra.acl2 FROM 
	`grouprole_acls` gra,  
    `groups` g,
    `roles` r
WHERE 
	gra.group_id is not NULL AND
  	gra.role_id is not NULL AND
    gra.group_id = g.id AND
    gra.role_id = r.id
    
UNION
SELECT gra.id, NULL as group_name, NULL as role_name, gra.imageable_type, gra.imageable_id, gra.acl2 FROM 
	`grouprole_acls` gra,  
    `groups` g,
    `roles` r
WHERE 
    gra.group_id IS NULL OR
    gra.role_id IS NULL

UNION
SELECT gra.id, g.name as group_name, NULL as role_name, gra.imageable_type, gra.imageable_id, gra.acl2 FROM 
	`grouprole_acls` gra,  
    `groups` g,
    `roles` r
WHERE 
    gra.role_id IS NULL
    
ORDER BY
	group_name, role_name, imageable_id, imageable_type