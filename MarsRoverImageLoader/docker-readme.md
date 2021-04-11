## build docker image
docker build -t rover-loader -f Dockerfile .


## create docker container
docker create --name mars-images -e API_KEY=uCagsYbUrNXe42YZyFxjlE9Zogyil4aVtc9f0DWb rover-loader

### passing API Key at run-time
docker run --env API_KEY=uCagsYbUrNXe42YZyFxjlE9Zogyil4aVtc9f0DWb rover-loader

## color-coded console output.
docker run --rm -it --env API_KEY=uCagsYbUrNXe42YZyFxjlE9Zogyil4aVtc9f0DWb -v /tmp/Photos:/app/Photos  rover-loader


### mounts for storing the photos?
docker run -v ${pwd}:/var/www mars-images


### volume hygiene
-v /var/ auto-mapped.
docker inspect to view mount locations.

```bash
docker rm -v <container> will remove the mounted volume.
```


```bash
docker: Error response from daemon: Mounts denied: 
The path /Photos is not shared from the host and is not known to Docker.
You can configure shared paths from Docker -> Preferences... -> Resources -> File Sharing.
See https://docs.docker.com/docker-for-mac for more info.
```

this one never seemed to work out:
docker run --name mars-images -it --env API_KEY=uCagsYbUrNXe42YZyFxjlE9Zogyil4aVtc9f0DWb -v ${pwd}/Photos:/app/Photos rover-loader
