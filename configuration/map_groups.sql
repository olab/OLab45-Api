SELECT mg.id, g.name as group_name, m.name FROM dev_olab.map_groups mg, groups g, maps m
WHERE m.id = map_id AND g.id = group_id order by g.name;