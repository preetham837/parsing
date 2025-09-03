from typing import List, Optional
import threading


class InMemoryDatabase:
    def __init__(self):
        self._lock = threading.Lock()
        # Add your data storage here
    
    # Add your database methods here


db = InMemoryDatabase()