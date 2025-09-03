import os
from typing import Tuple
from openai import AsyncOpenAI
from models import TodoCategory
from pydantic_settings import BaseSettings


class OpenAISettings(BaseSettings):
    openai_api_key: str = ""
    openai_model: str = "gpt-4o-mini"
    
    class Config:
        env_file = ".env"


settings = OpenAISettings()


class TodoClassifier:
    def __init__(self):
        if not settings.openai_api_key:
            raise ValueError("OPENAI_API_KEY environment variable is not set")
        
        self.client = AsyncOpenAI(api_key=settings.openai_api_key)
        self.model = settings.openai_model
    
    async def classify_todo(self, description: str) -> Tuple[TodoCategory, float]:
        system_prompt = """You are a task classifier. Classify the given todo item description into one of these categories:
        - Work: Tasks related to professional work, job, career, meetings, projects, business
        - Home: Tasks related to household, family, personal chores, home maintenance
        - Other: Tasks that don't clearly fit into Work or Home categories
        
        Respond with ONLY the category name (Work, Home, or Other) and a confidence score between 0 and 1.
        Format: CATEGORY|CONFIDENCE
        Example: Work|0.95"""
        
        try:
            response = await self.client.chat.completions.create(
                model=self.model,
                messages=[
                    {"role": "system", "content": system_prompt},
                    {"role": "user", "content": f"Classify this todo item: {description}"}
                ],
                temperature=0.3,
                max_tokens=20
            )
            
            result = response.choices[0].message.content.strip()
            
            # Parse the response
            if "|" in result:
                category_str, confidence_str = result.split("|")
                category_str = category_str.strip()
                confidence = float(confidence_str.strip())
            else:
                # Fallback if format is unexpected
                category_str = result.strip()
                confidence = 0.8
            
            # Map to enum
            category_map = {
                "Work": TodoCategory.WORK,
                "Home": TodoCategory.HOME,
                "Other": TodoCategory.OTHER
            }
            
            category = category_map.get(category_str, TodoCategory.OTHER)
            
            return category, confidence
            
        except Exception as e:
            print(f"Error classifying todo: {e}")
            # Default to Other with low confidence on error
            return TodoCategory.OTHER, 0.5


# Singleton instance
classifier = None

def get_classifier() -> TodoClassifier:
    global classifier
    if classifier is None:
        classifier = TodoClassifier()
    return classifier