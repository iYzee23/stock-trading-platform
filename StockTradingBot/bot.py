import requests
import time
import threading
import random

# BASE_URL = "http://localhost:8520/api"
BASE_URL = "http://localhost:19081/StockTradingPlatform/PlatformAPIV2/api"
REGISTER_URL = f"{BASE_URL}/auth/register"
LOGIN_URL = f"{BASE_URL}/auth/login"
BUY_URL = f"{BASE_URL}/trade/buy"
SELL_URL = f"{BASE_URL}/trade/sell"

users = {
    1: {
        "username": "botuser1",
        "name": "Bot User 1",
        "password": "password123",
        "email": "botuser1@microsoft.com",
        "country": "MNE"
    },
    2: {
        "username": "botuser2",
        "name": "Bot User 2",
        "password": "password123",
        "email": "botuser2@microsoft.com",
        "country": "SRB"
    },
    3: {
        "username": "botuser3",
        "name": "Bot User 3",
        "password": "password123",
        "email": "botuser3@microsoft.com",
        "country": "BIH"
    }
}

symbols = [
    "AAPL", "MSFT", "GOOG", "AMZN", "CSCO", "TSLA", "NVDA", "BRK.B", "JNJ", "V",
    "WMT", "PG", "JPM", "UNH", "MA", "DIS", "PYPL", "VZ", "ADBE", "NFLX"
]

def register(user_info):
    data = {
        "username": user_info["username"],
        "name": user_info["name"],
        "password": user_info["password"],
        "email": user_info["email"],
        "country": user_info["country"]
    }
    
    while True:
        try:
            response = requests.post(REGISTER_URL, json=data)
            if response.status_code == 200:
                return True
            else:
                time.sleep(10)
        except requests.exceptions.ConnectionError:
            time.sleep(10)

def login(user_info):
    data = {
        "username": user_info["username"],
        "password": user_info["password"]
    }
    response = requests.post(LOGIN_URL, json=data)
    if response.status_code == 200:
        token = response.json().get("token")
        return token
    else:
        return None

def trade(user_info, token):
    headers = {
        "Authorization": f"Bearer {token}"
    }
    username = user_info["username"]

    while True:
        symbol = random.choice(symbols)

        buy_quantity = random.randint(1, 10)
        buy_data = {
            "username": username,
            "stockSymbol": symbol,
            "quantity": buy_quantity
        }
        requests.post(BUY_URL, json=buy_data, headers=headers)
        time.sleep(10)
        
        sell_quantity = random.randint(1, buy_quantity)
        sell_data = {
            "username": username,
            "stockSymbol": symbol,
            "quantity": sell_quantity
        }
        requests.post(SELL_URL, json=sell_data, headers=headers)
        time.sleep(10)

def start_bot(user_info):
    if register(user_info):
        token = login(user_info)
        if token:
            trade_thread = threading.Thread(target=trade, args=(user_info, token))
            trade_thread.start()

if __name__ == "__main__":
    for user_id, user_info in users.items():
        start_bot(user_info)
        time.sleep(2)
