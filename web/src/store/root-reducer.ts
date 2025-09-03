import { combineReducers } from '@reduxjs/toolkit';

import { reducer as apiReducer, reducerPath } from './api';
import { idParsingApi } from './api/id-parsing-api';

export const rootReducer = combineReducers({
  [reducerPath]: apiReducer,
  [idParsingApi.reducerPath]: idParsingApi.reducer
});
