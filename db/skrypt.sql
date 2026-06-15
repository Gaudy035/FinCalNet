CREATE TABLE t_uzytkownik(
	id_uzytkownika SERIAL PRIMARY KEY,
	imie VARCHAR(30) NOT NULL,
	nazwisko VARCHAR(30) NOT NULL,
	email VARCHAR(100) UNIQUE NOT NULL,
	haslo VARCHAR(255) NOT NULL,
	data_zalozenia TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	czy_aktywny BOOLEAN DEFAULT TRUE,
	data_usuniecia TIMESTAMP
);

CREATE TABLE t_kategorie(
	id_kategorii SERIAL PRIMARY KEY,
	nazwa VARCHAR(30) UNIQUE NOT NULL
);

CREATE TABLE t_transakcje(
	id_transakcji SERIAL PRIMARY KEY,
	id_uzytkownika INTEGER NOT NULL REFERENCES 
		t_uzytkownik(id_uzytkownika) ON DELETE CASCADE,
	id_kategorii INTEGER REFERENCES
		t_kategorie(id_kategorii) ON DELETE CASCADE,
	typ VARCHAR(10) CHECK (typ IN ('wplyw', 'wydatek')),
	tytul VARCHAR(100) NOT NULL,
	opis TEXT,
	kwota NUMERIC(12,2) NOT NULL,
	metoda VARCHAR(10) CHECK (metoda IN ('gotowka', 'przelew')),
	konto VARCHAR(50),
	wlasciciel_konta VARCHAR(100),
	data DATE NOT NULL DEFAULT CURRENT_DATE
);

CREATE TABLE t_t_powtarzalne(
	id_t_powtarzalnej SERIAL PRIMARY KEY,
	id_uzytkownika INTEGER NOT NULL REFERENCES 
		t_uzytkownik(id_uzytkownika) ON DELETE CASCADE,
	id_kategorii INTEGER REFERENCES
		t_kategorie(id_kategorii) ON DELETE CASCADE,
	typ VARCHAR(10) CHECK (typ IN ('wplyw', 'wydatek')),
	tytul VARCHAR(100) NOT NULL,
	opis TEXT,
	kwota NUMERIC(12,2) NOT NULL,
	metoda VARCHAR(10) CHECK (metoda IN ('gotowka', 'przelew')),
	konto VARCHAR(50),
	wlasciciel_konta VARCHAR(100),
	co_ile VARCHAR(10),
	nastepny_termin DATE NOT NULL,
	czy_aktywna BOOLEAN DEFAULT TRUE
);

INSERT INTO t_kategorie (nazwa) VALUES ('rozrywka');
INSERT INTO t_kategorie (nazwa) VALUES ('transport');
INSERT INTO t_kategorie (nazwa) VALUES ('jedzenie');
INSERT INTO t_kategorie (nazwa) VALUES ('oplaty');
INSERT INTO t_kategorie (nazwa) VALUES ('pozostale');
