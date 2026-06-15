export default interface TPowtarzalna {
  id_t_powtarzalnej: number;
  id_uzytkownika: number;
  id_kategorii: number | null;
  typ: 'wplyw' | 'wydatek';
  tytul: string;
  opis: string | null;
  kwota: number;
  metoda: string;
  konto: string | null;
  wlasciciel_konta: string | null;
  nastepny_termin: string;
  co_ile: string;
  czy_aktywna: boolean | number;
}
