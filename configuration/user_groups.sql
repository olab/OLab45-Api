SELECT ug.id, ug.user_id, u.username, g.name as group_name, r.name as role_name 
FROM olab_dev.user_groups ug, groups g, roles r, users u 
WHERE u.id = ug.user_id and ug.role_id = r.id and ug.group_id = g.id
order by u.username