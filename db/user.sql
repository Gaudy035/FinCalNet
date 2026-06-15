CREATE USER fin_calc_api WITH PASSWORD 'fin_calc_pass';

GRANT CONNECT ON DATABASE fin_calc_db TO fin_calc_api;

GRANT USAGE ON SCHEMA public TO fin_calc_api;

GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES 
	IN SCHEMA public TO fin_calc_api;

GRANT USAGE, SELECT ON ALL SEQUENCES 
	IN SCHEMA public TO fin_calc_api;