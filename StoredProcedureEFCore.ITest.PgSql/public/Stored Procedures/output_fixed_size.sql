CREATE OR REPLACE FUNCTION output_fixed_size(OUT fixed_size VARCHAR(255)) AS $$
BEGIN
    fixed_size = 'Jambon Beurre';
END
$$ LANGUAGE plpgsql;
