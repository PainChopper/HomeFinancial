DROP FUNCTION IF EXISTS public.retry_test_raise() CASCADE;

-- Функция, симулирующая ошибку deadlock (SQLSTATE 40P01)
CREATE OR REPLACE FUNCTION retry_test_raise()
    RETURNS trigger AS
$$
BEGIN
    -- Если имя категории равно 'Такси', кидаем ошибку
    IF NEW.name = 'Такси' THEN
        RAISE EXCEPTION 'Simulated deadlock failure'
            USING ERRCODE = '40P01';
END IF;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_retry_test_raise
    BEFORE INSERT OR UPDATE ON file_transactions
    FOR EACH ROW
EXECUTE FUNCTION retry_test_raise();