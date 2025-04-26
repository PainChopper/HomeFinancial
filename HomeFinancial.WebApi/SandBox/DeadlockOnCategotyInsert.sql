DROP FUNCTION IF EXISTS public.retry_test_raise() CASCADE;

-- Функция, симулирующая ошибку deadlock (SQLSTATE 40P01)
CREATE OR REPLACE FUNCTION retry_test_raise()
    RETURNS trigger AS
$$
BEGIN
    -- Если имя категории равно 'RetryTest', кидаем ошибку
    IF NEW.name = 'RetryTest' THEN
        RAISE EXCEPTION 'Simulated deadlock failure'
            USING ERRCODE = '40P01';
END IF;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;