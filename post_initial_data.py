import requests
import time
# 
names = ['ropf', 'adho', 'adho', 'ropf']
messages = ['\"Hello, BDSA students!\"', '\"Welcome to the course!\"', '\"I hope you had a good summer.\"', '\"Cheeping cheeps on Chirp :)\"']
times = [1690891760, 1690978778, 1690979858, 1690981487]

#resp = requests.post('https://bdsagroup26chirpremotedb.azurewebsites.net/cheep', json={"author": "testAuthor", "message": "testMessage", "timestamp": int(time.time())})

for i in range(4):
    resp = requests.post('https://bdsagroup26chirpremotedb.azurewebsites.net/cheep', json={"Author": names[i], "Message": messages[i], "Timestamp": times[i]})
    print(resp.status_code)
