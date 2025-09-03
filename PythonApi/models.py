from pydantic import BaseModel
from typing import Optional, Literal
from enum import Enum


class TodoCategory(str, Enum):
    WORK = "Work"
    HOME = "Home"
    OTHER = "Other"


class TodoClassificationRequest(BaseModel):
    description: str


class TodoClassificationResponse(BaseModel):
    description: str
    category: TodoCategory
    confidence: float