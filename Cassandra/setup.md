Installation and startup:
https://cassandra.apache.org/doc/latest/cassandra/getting_started/installing.html

https://docs.datastax.com/en/cassandra-oss/3.x/cassandra/initialize/referenceStartCservice.html

Commands:

```bash
echo "deb https://debian.cassandra.apache.org 41x main" | sudo tee -a /etc/apt/sources.list.d/cassandra.sources.list

curl https://downloads.apache.org/cassandra/KEYS | sudo apt-key add 

sudo apt-get update

sudo apt-get install cassandra

sudo service cassandra start

nodetool status

pip install cqlsh

cqlsh
```

Tools:
https://dbschema.com/download.html

