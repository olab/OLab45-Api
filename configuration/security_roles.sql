SELECT sr.id, imageable_type, imageable_id, acl, bin(acl2), g.name as GroupName, r.name as RoleName
FROM security_roles sr, `groups` g, roles r
WHERE sr.group_id = g.id and sr.role_id = r.id
ORDER BY id DESC