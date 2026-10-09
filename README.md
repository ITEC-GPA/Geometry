# Geometry

## Curve parametriche 3D

`GPC.Geometry.Curve3d` rappresenta un percorso orientato con un dominio parametrico
esplicito. Le implementazioni disponibili sono:

| Tipo | Geometria |
| --- | --- |
| `LineCurve3d` | Segmento orientato |
| `ArcCurve3d` | Arco circolare con angolo percorso positivo o negativo, fino a un giro completo |
| `PolylineCurve3d` | Polilinea nello spazio, anche non planare |
| `PolyCurve3d` | Sequenza ordinata di curve connesse, anche annidate |

La classe base resta estendibile per altre rappresentazioni; non contiene codice
specifico per NURBS. I tipi preesistenti (`Line3d`, `Circle3d`, `Circle3dArc`,
`Polygon3d`, `Shape`) conservano firme e comportamento.

### Parametri e lunghezza

- Il dominio iniziale e' `[0,1]`. `SetDomain(new CurveInterval(a, b))` cambia i
  parametri senza modificare la geometria. Gli estremi devono essere finiti e
  strettamente crescenti; `default(CurveInterval)` non e' valido.
- `PointAt(t)` e `TangentAt(t)` usano il parametro; `PointAtLength(s)` usa una
  distanza fisica dall'inizio. `ParameterAtLength(s)` e `LengthAt(t)` collegano
  le due rappresentazioni.
- Linee e archi hanno velocita' parametrica costante. Nelle polilinee e nelle
  curve composte ogni tratto occupa inizialmente un intervallo uguale, anche se
  ha una lunghezza diversa. Il punto a parametro medio puo' quindi differire
  dal punto a meta' lunghezza.
- `Trim(a, b)` conserva il dominio `[a,b]` e la parametrizzazione del tratto
  originale. `Split(t)` richiede un parametro interno. Entrambi restituiscono
  copie indipendenti. `Reversed()` mantiene il dominio e inverte il percorso.
- Agli spigoli `TangentAt(t)` sceglie il tratto uscente, eccetto all'estremo
  finale. `CurveEvaluationSide.Below` e `.Above` selezionano esplicitamente
  il lato rispetto ai parametri crescenti.
- I parametri esterni al dominio e le distanze esterne a `[0, Length]` generano
  un'eccezione; non viene effettuata extrapolazione. `ClosestParameter(point)`
  cerca invece sul percorso finito, includendone gli estremi.

Queste sono convenzioni parametriche della libreria, ispirate al modello di
curva parametrica di Rhino; domini iniziali e firme non sono una replica della
sua API.

```csharp
using GPC.Geometry;

var line = new LineCurve3d(new Point3d(0, 0, 0), new Point3d(2, 0, 0));
var arc = new ArcCurve3d(
    new Point3d(2, 1, 0),       // centro
    new Vector3d(0, 0, 1),      // normale del piano
    new Vector3d(0, -1, 0),     // direzione dal centro al punto iniziale
    1, System.Math.PI / 2);     // raggio, angolo percorso
Curve3d path = new PolyCurve3d(new Curve3d[] {
    line, arc, new LineCurve3d(arc.EndPoint, new Point3d(3, 4, 0))
});

path.SetDomain(new CurveInterval(10, 20));
Point3d atParameter = path.PointAt(15);
Point3d atHalfLength = path.PointAtLength(path.Length / 2);
Curve3d trimmed = path.Trim(12, 18);
Point3d[] vertices = path.ToPolyline(tolerance: 0.001, maxSegmentLength: 0.25);
Line3d[] segments = path.ToLineSegments(tolerance: 0.001);
```

### Copie, raccordi e tolleranze

Le curve copiano punti e geometrie in ingresso. Anche punti, vertici e segmenti
restituiti sono copie: modificarli non cambia la curva. `Move` e `SetDomain`
modificano l'istanza; le altre operazioni geometriche producono nuovi oggetti.
Non e' previsto l'accesso concorrente durante una modifica.

`DuplicateCurve()` e `Clone()` producono una copia geometrica profonda, con un
nuovo `Guid` e lo stesso riferimento a `Tag`. La serializzazione segue il
contratto esistente della libreria: conserva `Guid` e dominio, esclude `Tag`.
L'uguaglianza delle curve considera tipo concreto, verso e dominio, oltre ai
dati geometrici; non e' un confronto del solo luogo geometrico. L'hash e'
costante per mantenere il contratto dell'uguaglianza con tolleranza.

`PolyCurve3d` richiede segmenti gia' ordinati e orientati. Verifica i raccordi
entro `JoinTolerance`, senza modificare gli estremi. Eventuali piccoli vuoti
ammessi nei raccordi sono esclusi dalla lunghezza della curva e rappresentati
da segmenti espliciti nella discretizzazione. Una tolleranza di discretizzazione
inferiore al vuoto viene rifiutata. In corrispondenza di questi raccordi la
continuita' e' garantita entro `JoinTolerance`; per una continuita' esatta occorre
fornire estremi coincidenti. La chiusura delle polilinee usa la tolleranza di
Geometry, quella delle curve composte usa `JoinTolerance`; un arco e' chiuso
solo se compie un giro completo.

`ToPolyline` restituisce un array di punti, conserva gli spigoli e controlla
l'errore delle corde; `maxSegmentLength` limita anche la lunghezza dei segmenti.
Richieste che superano un milione di segmenti sono rifiutate esplicitamente.
La ricerca del tratto e l'inversione della lunghezza nelle curve composte sono
logaritmiche; la ricerca del punto piu' vicino esamina tutti i tratti.

### Conversioni e mesh

Gli adattatori `ToCurve3d()` copiano `Line3d`, `Circle3d`, `Circle3dArc`,
`Polygon2d` e `Polygon3d`. Per i poligoni restituiscono il contorno chiuso.
Un arco legacy con punto di passaggio usa quel punto per distinguere il percorso;
senza punto di passaggio viene usato l'arco minore. Un semicerchio legacy senza
normale recuperabile richiede la costruzione esplicita di `ArcCurve3d`.

`ToPolygon3d(tolerance, maxSegmentLength)` converte un contorno chiuso e planare
in un poligono approssimato, senza proiezioni o correzioni automatiche degli
estremi. Si puo' quindi costruire una `Shape` e usare i mesher esistenti; area,
inerzie e mesh restano quelle del contorno poligonale approssimato.

`GMesh.Generate` accetta inoltre `Curve3d` nelle geometrie incorporate nelle
superfici. Le opzioni aggiuntive sono:

- `CurveChordTolerance`: massimo errore delle corde, nelle unita' geometriche
  originali; valore iniziale `GeometryBase.Tolerance`.
- `CurveMaxSegmentLength`: massima lunghezza delle corde, nelle stesse unita';
  valore iniziale infinito.

Le dimensioni locali della mesh associate alla curva sono trasferite ai segmenti;
per vincoli coincidenti viene usata la dimensione piu' piccola. La mappa
`status.EmbeddedGeometriesVertexMap[mesh][curveOriginale]` contiene gli identificativi
dei nodi recuperati, ordinati secondo il percorso e senza duplicati. Rimangono
disponibili anche le voci dei segmenti. Per curve parzialmente esterne alla
superficie la mappa contiene solo le parti effettivamente incorporate. Le
statistiche delle linee incorporate contano i segmenti generati.

Le funzioni di calcolo in Model e Checker e i formati dei progetti ANTHEA non
sono migrati alla nuova rappresentazione. Offset, intersezioni generali tra
curve e NURBS non fanno parte di questa implementazione.

### Verifiche

La suite `CurveFeature100Tests`, categoria MSTest `CurveFeature100`, comprende
esattamente 100 casi scoperti individualmente: 20 per i domini e 20 per ciascuna
delle quattro categorie di curve. Copre scale e domini diversi, orientamento,
geometria spaziale, inversione della lunghezza, tagli, discretizzazione, copie e
serializzazione. Si aggiungono le regressioni mirate in `Curve3dTest` e i test
di integrazione Gmsh in `CurveMeshingTest`.

Eseguire la soluzione in Release con Visual Studio/MSBuild e il test runner x64.
Il filtro `TestCategory=CurveFeature100` esegue solo i 100 casi dedicati; senza
filtro viene eseguita anche tutta la suite preesistente.
