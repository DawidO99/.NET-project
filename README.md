# 🧑‍🔧 System zarządzania warsztatem samochodowym 2.0

## 🚀 Integracja Ciągła (CI/CD) z GitHub Actions

Projekt wykorzystuje GitHub Actions do automatyzacji procesów Continuous Integration (CI).
Każdy push oraz Pull Request do gałęzi `main` i `develop` automatycznie uruchamia workflow, który wykonuje następujące kroki:

* **Przywracanie zależności (.NET Restore):** Pobiera wszystkie wymagane pakiety NuGet.
* **Budowanie projektu (.NET Build):** Kompiluje kod źródłowy aplikacji, sprawdzając poprawność składni i zależności.
* **Uruchamianie testów jednostkowych (.NET Test):** Wykonuje wszystkie testy jednostkowe zdefiniowane w projekcie (jeśli takie istnieją), zapewniając, że nowe zmiany nie wprowadzają regresji.

**Link do statusu CI:** [Link do statusu Twojego workflow w GitHub Actions]
