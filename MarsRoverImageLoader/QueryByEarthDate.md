## Querying by Earth date

| Parameter	| Type | Default | Description |
| --------- | ---- | ------- | ----------- |
| earth_date | YYYY-MM-DD | none | corresponding date on earth for the given sol|
| camera | string | all	| see table above for abbreviations|
| page | int | 1 |	25 items per page returned |
| api_key |	string | DEMO_KEY |	api.nasa.gov key for expanded usage |

### Example query
[https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos?earth_date=2015-6-3&api_key=DEMO_KEY](https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos?earth_date=2015-6-3&api_key=DEMO_KEY)

