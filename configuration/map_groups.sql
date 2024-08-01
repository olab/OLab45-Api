SELECT m.id, m.name, g.name
FROM 
	maps m, 
    map_groups mg,
    `groups` g
WHERE 
	m.id = mg.map_id AND
    g.id = mg.group_id
ORDER BY m.id;
    