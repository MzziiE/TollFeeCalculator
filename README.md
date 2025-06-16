# Trängselskatt Kalkylator för Göteborg

En modern och robust .NET-applikation för att beräkna trängselskatt för fordon som passerar betalstationer i Göteborg.

## 📋 Innehållsförteckning

- [Översikt](#översikt)
- [Funktioner](#funktioner)
- [Arkitektur](#arkitektur)
- [Installation och Körning](#installation-och-körning)
- [Användning](#användning)
- [Tekniska Detaljer](#tekniska-detaljer)
- [Förbättringar från Original](#förbättringar-från-original)
- [Tester](#tester)
- [Framtida Förbättringar](#framtida-förbättringar)

## 🎯 Översikt

Denna applikation implementerar Göteborgs trängselskattsystem enligt gällande regler. Systemet hanterar olika fordonstyper, tidsbaserade avgifter, helgdagar och särskilda regler för avgiftsfri trafik.

### Avgiftsstruktur

| Tidsperiod  | Avgift |
| ----------- | ------ |
| 06:00–06:29 | 8 kr   |
| 06:30–06:59 | 13 kr  |
| 07:00–07:59 | 18 kr  |
| 08:00–08:29 | 13 kr  |
| 08:30–14:59 | 8 kr   |
| 15:00–15:29 | 13 kr  |
| 15:30–16:59 | 18 kr  |
| 17:00–17:59 | 13 kr  |
| 18:00–18:29 | 8 kr   |
| 18:30–05:59 | 0 kr   |

## ✨ Funktioner

### Kärnfunktioner

- **Tidsbaserad avgiftsberäkning**: Automatisk beräkning baserat på passeringstidpunkt
- **60-minutersregeln**: Fordon som passerar flera stationer inom 60 minuter debiteras endast en gång (högsta avgiften)
- **Daglig maxgräns**: Maximalt 60 kr per fordon och dag
- **Helgdagshantering**: Integrerat med svenska helgdagar via API och fallback-system
- **Fordonstypsstöd**: Olika regler för olika fordonstyper
- **Kommunala fordon**: Särskild hantering för kommunala fordon

### Avgiftsfria situationer

- Lördagar och söndagar
- Helgdagar och helgdagsaftnar
- Juli månad (semestermånad)
- Avgiftsfria fordonstyper: Motorcykel, Traktor, Utryckningsfordon, Diplomatfordon, Utländska fordon, Militärfordon, Buss

## 🏗️ Arkitektur

Applikationen följer moderna .NET-principer med tydlig separation av concerns:

```
TollFeeCalculator/
├── Config/
│   └── TollFeeConfig.cs          # Konfiguration för avgifter och regler
├── Enums/
│   └── VehicleOwnership.cs       # Fordonsägarskap enum
├── Exceptions/
│   ├── InvalidPassageDatesException.cs
│   └── MultipleDaysException.cs
├── Extensions/
│   ├── ServiceCollectionExtensions.cs  # DI-konfiguration
│   └── EnumerableExtensions.cs
├── Models/
│   ├── Holiday.cs                # Helgdagsmodell
│   ├── IHolidayService.cs        # Helgdagstjänst interface
│   └── TollFeeInterval.cs        # Avgiftsintervall
├── Services/
│   └── HolidayService.cs         # Helgdagstjänst implementation
├── Vehicles/
│   ├── IVehicle.cs               # Fordonsinterface
│   ├── Car.cs                    # Bil implementation
│   ├── Bus.cs                    # Buss implementation
│   └── Motorbike.cs              # Motorcykel implementation
├── ITollCalculator.cs            # Huvudinterface
├── TollCalculator.cs             # Huvudimplementation
└── Program.cs                    # Applikationens startpunkt
```

### Design Patterns

- **Dependency Injection**: För löst kopplad kod och testbarhet
- **Strategy Pattern**: För olika fordonstyper
- **Repository Pattern**: För helgdagshantering
- **SOLID Principles**: Följs genomgående i designen

## 🚀 Installation och Körning

### Förutsättningar

- .NET 8.0 eller senare
- Internetanslutning (för helgdags-API, har fallback)

### Steg för steg

1. **Klona repositoriet**

   ```bash
   git clone [din-repository-url]
   cd TollFeeCalculator
   ```

2. **Återställ NuGet-paket**

   ```bash
   dotnet restore
   ```

3. **Bygg projektet**

   ```bash
   dotnet build
   ```

4. **Kör applikationen**
   ```bash
   dotnet run
   ```

## 📖 Användning

### Grundläggande användning

Applikationen kör automatiskt en demonstration som visar:

- Olika fordonstyper (Bil, Motorcykel, Buss)
- Helgdagshantering (1 maj 2013)
- 60-minutersregeln
- Daglig maxgräns
- Avgiftsfri månad (juli)

### Programmatisk användning

```csharp
// Skapa tjänster
var services = new ServiceCollection();
services.Initialize();
var serviceProvider = services.BuildServiceProvider();
var calculator = serviceProvider.GetRequiredService<ITollCalculator>();

// Skapa fordon
var car = new Car();

// Definiera passager
var passages = new List<DateTime>
{
    new(2013, 5, 15, 7, 0, 0),   // 18 kr
    new(2013, 5, 15, 15, 45, 0), // 18 kr
    new(2013, 5, 15, 18, 0, 0)   // 8 kr
};

// Beräkna avgifter
var fees = await calculator.GetTollFees(car, passages);
```

## 🔧 Tekniska Detaljer

### Helgdagshantering

Systemet använder en tre-stegs fallback-strategi:

1. **Primär**: PublicHoliday NuGet-bibliotek
2. **Sekundär**: Dagsmart API (https://api.dagsmart.se)
3. **Fallback**: Hårdkodade svenska helgdagar

### Caching

- **Helgdagar**: Cachas per år för prestanda
- **Avgiftsfria dagar**: Cachas för att undvika upprepade beräkningar

### Felhantering

- Robusta `ArgumentNullException` för null-värden
- Anpassade exceptions för affärslogik
- Graceful degradation vid API-fel

### Prestanda

- Asynkron programmering för API-anrop
- Effektiv gruppering av passager per dag
- Minimal API-anrop genom intelligent caching

## 🔄 Förbättringar från Original

### Strukturella förbättringar

- **Modularisering**: Uppdelning i logiska mappar och namespaces
- **Dependency Injection**: Moderniserat för testbarhet och underhållbarhet
- **Async/Await**: Asynkron helgdagshantering
- **Configuration**: Centraliserad konfiguration av avgifter och regler

### Kodkvalitet

- **SOLID Principles**: Följs konsekvent
- **Clean Code**: Tydliga metodnamn och ansvarsområden
- **Error Handling**: Robusta felhantering med anpassade exceptions
- **Validation**: Omfattande validering av indata

### Funktionalitet

- **Flexibel fordonshantering**: Stöd för kommunala fordon
- **Bättre helgdagshantering**: Flera fallback-strategier
- **Prestanda**: Caching och optimerade algoritmer
- **Extensibilitet**: Lätt att lägga till nya fordonstyper eller regler

### Testbarhet

- **Interfaces**: Alla beroenden är abstraherade
- **Dependency Injection**: Enkelt att mocka dependencies
- **Pure Functions**: Logik separerad från I/O-operationer

## 🧪 Tester

För att köra tester (när de implementeras):

```bash
dotnet test
```

### Testscenarier som bör täckas

- Grundläggande avgiftsberäkning
- 60-minutersregeln
- Helgdagshantering
- Avgiftsfria fordonstyper
- Daglig maxgräns
- Felhantering

## 🚀 Framtida Förbättringar

### Kortsiktiga förbättringar

- [ ] Implementera omfattande enhetstester
- [ ] Lägg till integrationstester
- [ ] Implementera logging med ILogger
- [ ] Lägg till konfigurationsstöd via appsettings.json

### Långsiktiga förbättringar

- [ ] Web API för extern användning
- [ ] Databas för att lagra historiska data
- [ ] Rapportering och statistik
- [ ] Administrationsgränssnitt
- [ ] Stöd för flera städer/regioner
- [ ] Realtidsintegration med betalstationer

### Tekniska förbättringar

- [ ] Implementera Circuit Breaker pattern för API-anrop
- [ ] Lägg till metrics och monitoring
- [ ] Containerisering med Docker
- [ ] CI/CD pipeline
- [ ] Performance benchmarking

## 🤝 Bidrag

Detta projekt utvecklades som en del av en teknisk utvärdering för att visa:

- Förståelse för objektorienterad programmering
- Förmåga att strukturera och refaktorera kod
- Kunskap om moderna .NET-utvecklingsprinciper
- Problemlösningsförmåga och teknisk kreativitet

## 📝 Licens

Detta projekt är utvecklat för utvärderingssyften och demonstrerar implementering av Göteborgs trängselskattsystem.

---

_Utvecklat som en del av teknisk bedömning för att visa färdigheter inom .NET-utveckling och systemarkitektur._
