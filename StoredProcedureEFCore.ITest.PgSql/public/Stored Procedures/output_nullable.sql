CREATE OR REPLACE FUNCTION output_nullable(OUT nullable INT) AS $$
BEGIN
    nullable = null;
END
$$ LANGUAGE plpgsql;
