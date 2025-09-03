from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import RedirectResponse
from models import TodoClassificationRequest, TodoClassificationResponse
from openai_service import get_classifier

app = FastAPI(title="Movies API", version="v1", docs_url="/swagger", redoc_url="/redoc")
app.title = "Movies API"
app.version = "v1"
app.description = "Movies API with Todo Classification"

# Configure CORS to allow all origins
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Allow all origins
    allow_credentials=True,
    allow_methods=["*"],  # Allow all HTTP methods
    allow_headers=["*"],  # Allow all headers
)

# Send interactive user to swagger page by default
@app.get("/")
async def redirect_to_swagger():
    return RedirectResponse(url="/swagger")

# Todo classification endpoint
@app.post("/api/todos/classify", response_model=TodoClassificationResponse)
async def classify_todo(request: TodoClassificationRequest):
    """
    Classify a todo item description into one of the categories: Work, Home, or Other.
    Uses OpenAI API for classification.
    """
    try:
        classifier = get_classifier()
        category, confidence = await classifier.classify_todo(request.description)
        
        return TodoClassificationResponse(
            description=request.description,
            category=category,
            confidence=confidence
        )
    except ValueError as e:
        raise HTTPException(status_code=500, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Classification failed: {str(e)}")