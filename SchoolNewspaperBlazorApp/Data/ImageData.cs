namespace SchoolNewspaperBlazorApp.Data;

// Zdjęcie wczytane w całości do pamięci (zaraz po wybraniu pliku).
// Dzięki temu zapis nie zależy już od komponentu <InputFile>, który mógł w międzyczasie zniknąć ze strony.
public sealed record ImageData(string FileName, string Extension, byte[] Bytes, string PreviewUrl);
