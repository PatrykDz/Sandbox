## TODO

## Setup Video
https://www.youtube.com/watch?v=n_IQq3pze0s

# Kafka UI - koncil
```bash
docker run -d -p 80:8080 -e bootstrapServers="kafka:9092" -e kouncil.auth.active-provider="inmemory" consdata/kouncil:latest
```
