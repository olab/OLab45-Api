SELECT
	mgr.id,
	mgr.map_id,
    CONCAT( m.name, " (", mgr.map_id, ")" ) as `map`,
    IF( mgr.group_id IS NOT NULL, ( SELECT CONCAT( name, " (", id, ")" ) from `groups` WHERE id = mgr.group_id ), null ) as `group`,
    IF( mgr.role_id IS NOT NULL, ( SELECT CONCAT( name, " (", id, ")" ) from `roles` WHERE id = mgr.role_id ), null ) as `role`
FROM 
	map_grouproles mgr, 
	maps m
WHERE 
	mgr.map_id = m.id
  
ORDER BY map, `group`, `role`