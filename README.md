# HobbyAPI
Esse projeto com como objetivo gerenciar, criar, mudar e deletar hábitos.

# Autention
Esse projeto tem uma persistência de sessão simples que apenas exige seu nickname em cada rota
Exemplo: GET: api/habits/{seu nickname}

# endpoints 
POST: https://localhost:7004/api/habits 
POST: https://localhost:7004/api/logs
GET: https://localhost:7004/api/habits
GET: https://localhost:7004/api/habits/{id}
GET: https://localhost:7004/api/stats/weekly 
GET: https://localhost:7004/api/badges
PUT: https://localhost:7004/api/habits/{id} 
PUT: https://localhost:7004/api/VerifyAmount
DELETE: https://localhost:7004/api/habits/{id}

# returns
200 -> Ok
204 -> NoContent
201 -> Created

# erros
400 -> BadRequest
404 -> NotFound
