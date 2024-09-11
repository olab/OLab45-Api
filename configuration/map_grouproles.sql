SELECT
	mgr.id,
    CONCAT( m.name, " (", mgr.map_id, ")" ) as `map`,
    ifnull( mgr.group_id, CONCAT( g.name, " (", mgr.group_id, ")" ) ) as `group`,
    ifnull( mgr.role_id, CONCAT( r.name, " (", mgr.role_id, ")" ) ) as `role`
FROM 
	map_grouproles mgr, 
	maps m,
    `groups` g,
    `roles` r    
WHERE 
	mgr.map_id = m.id AND
    mgr.group_id IS NOT NULL AND
    mgr.role_id IS NOT NULL AND    
    mgr.group_id = g.id AND
    mgr.role_id = r.id
    
UNION

SELECT
	mgr.id,
    CONCAT( m.name, " (", mgr.map_id, ")" ) as `map`,
    CONCAT( g.name, " (", mgr.group_id, ")" ) as `group`,
    mgr.role_id as `role`
FROM 
	map_grouproles mgr, 
	maps m,
    `groups` g   
WHERE 
	mgr.map_id = m.id AND
    mgr.group_id IS NOT NULL AND
    mgr.role_id IS NULL AND    
    mgr.group_id = g.id 
    
UNION

SELECT
	mgr.id,
    CONCAT( m.name, " (", mgr.map_id, ")" ) as `map`,
    CONCAT( g.name, " (", mgr.group_id, ")" ) as `group`,
    CONCAT( r.name, " (", mgr.role_id, ")" ) as `role`
FROM 
	map_grouproles mgr, 
	maps m,
    `groups` g,
    `roles` r    
WHERE 
	mgr.map_id = m.id AND
    mgr.group_id IS NOT NULL AND
    mgr.role_id IS NOT NULL AND    
    mgr.group_id = g.id AND
    mgr.role_id = r.id
ORDER BY map, `group`