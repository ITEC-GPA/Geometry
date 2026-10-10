# Regole per gli agenti (librerie GPC: Geometry)

- **Versioni**: le assegna l'utente. Non cambiare le versioni di GPCGeometry, DelaunayMesh o GMesh senza averglielo chiesto; sui branch di lavoro restano quelle di master.
- **Rilascio**: un solo treno per Utilities, Geometry, Model e Checker, descritto in `../Checker/build/RILASCIO.txt`. `bin/Release` si ricompila e si committa solo nel commit di versione su master; poi Model aggiorna versioni e SHA-256 in `build/dependencies.props`.
- **Mesh**: le mesh DelaunayMesh delle sezioni usuali devono restare identiche bit per bit (catture congelate in Checker e ANTHEA).
- **Gmsh** (GMsh.Net, UnsafeEx, `gmsh-*.dll`, GMesh) solo negli unit test e negli strumenti di sviluppo, mai nelle DLL distribuite con le librerie o con ANTHEA.
- **Git**: ogni lavoro parte da master aggiornato su un branch dedicato; merge su master e push solo con l'ok dell'utente.
