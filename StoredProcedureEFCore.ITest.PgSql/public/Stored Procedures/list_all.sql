CREATE OR REPLACE FUNCTION list_all(lim BIGINT = 9223372036854775807)
RETURNS TABLE (
  id                   BIGINT,
  name                 VARCHAR(255),
  date                 TIMESTAMPTZ,
  active               BOOLEAN,
  name_with_underscore INT,
  extra_column         INT
)
LANGUAGE sql
AS $$
    SELECT *, 5 AS extra_column FROM table_1 LIMIT lim;
$$;
