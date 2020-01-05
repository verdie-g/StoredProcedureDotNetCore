CREATE OR REPLACE FUNCTION list_not_all(lim BIGINT = 9223372036854775807)
RETURNS TABLE (
  id                   BIGINT,
  name                 VARCHAR(255),
  extra_column         INT
)
LANGUAGE sql
AS $$
    SELECT id, name, 5 AS extra_column FROM Table_1 LIMIT lim;
$$;
