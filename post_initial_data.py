import requests
import time
# 
names = ['ropf', 'adho', 'adho', 'ropf']
messages = ['\"Hello, BDSA students!\"', '\"Welcome to the course!\"', '\"I hope you had a good summer.\"', '\"Cheeping cheeps on Chirp :)\"']
times = [1690891760, 1690978778, 1690979858, 1690981487]

# https://bdsagroup26chirpremotedb.azurewebsites.net
# http://localhost:5000


resp = requests.post('https://bdsagroup26chirpremotedb.azurewebsites.net/cheep', json={"Author": "testauthor", "Message": "Azure persistent database test message 123", "Timestamp": 1690982000})
print(resp.status_code)
