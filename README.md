## Lab 6 - Backend Only

# LaLiga Tracker API

## Documentación del API
- [Documentación Swagger](http://localhost:8080/swagger)
- [Colección Postman](https://www.postman.com/collections/...) o [Hoppscotch](https://hoppscotch.io/...)

## Endpoints PATCH
```json
{
  "PATCH /api/matches/{id}/goals": "Incrementa el contador de goles",
  "PATCH /api/matches/{id}/yellowcards": "Incrementa tarjetas amarillas",
  "PATCH /api/matches/{id}/redcards": "Incrementa tarjetas rojas",
  "PATCH /api/matches/{id}/extratime?minutes=X": "Establece minutos de tiempo extra"
}
```

## Ejecución
```bash
docker-compose up --build
```

## Modelo de Datos
```csharp
public class Match
{
    public int Id { get; set; }
    public string HomeTeam { get; set; }
    public string AwayTeam { get; set; }
    public string MatchDate { get; set; }
    public int Goals { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public int ExtraTimeMinutes { get; set; }
}
```

# Prueba del Frontend Funcional
![image](https://github.com/user-attachments/assets/86ba162b-b1b1-475d-a581-b343bb3e0875)
![image](https://github.com/user-attachments/assets/fc96d465-30b8-4a02-bbd6-d11dc51ee42f)
