export default interface Transakcja {
  id_transakcji: number;
  id_uzytkownika: number;
  id_kategorii: number | null;
  typ: 'wplyw' | 'wydatek';
  tytul: string;
  opis: string | null;
  kwota: number;
  metoda: string;
  konto: string | null;
  wlasciciel_konta: string | null;
  data: string;
}
