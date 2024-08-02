SELECT 
    gra.id,
    gra.group_id as `group`,
    gra.role_id as `role`,    
    gra.imageable_type, 
    gra.imageable_id, 
    gra.acl2 
FROM 
	`grouprole_acls` gra
WHERE 
	gra.group_id is NULL AND
  	gra.role_id is NULL     
UNION

SELECT 
    gra.id,
    CONCAT( g.name, " (", g.id, ")" ) as `group`,
    CONCAT( r.name, " (", r.id, ")" ) as `role`,    
    gra.imageable_type, 
    gra.imageable_id, 
    gra.acl2 
FROM 
	`grouprole_acls` gra,  
    `groups` g,
    `roles` r
WHERE 
	gra.group_id is not NULL AND
  	gra.role_id is not NULL AND
    gra.group_id = g.id AND
    gra.role_id = r.id
    
UNION
SELECT 
    gra.id,
    gra.group_id as `group`, 
    CONCAT( r.name, " (", r.id, ")" ) as `role`,    
    gra.imageable_type, 
    gra.imageable_id, 
    gra.acl2 
FROM 
	`grouprole_acls` gra,  
    `roles` r
WHERE 
    gra.group_id IS NULL AND
    gra.role_id IS NOT NULL AND
    gra.role_id = r.id    

UNION
SELECT 
    gra.id,
    CONCAT( g.name, " (", g.id, ")" ) as `group`,
    gra.role_id as role, 
    gra.imageable_type, 
    gra.imageable_id, 
    gra.acl2 
FROM 
	`grouprole_acls` gra,  
    `groups` g
WHERE 
    gra.group_id IS NOT NULL AND
    gra.role_id IS NULL AND
    gra.group_id = g.id 
ORDER BY
	`group`, role, imageable_id, imageable_type