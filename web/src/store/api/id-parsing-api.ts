/* eslint-disable */
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

// Define types for ID parsing API
export type ParseIdApiResponse = {
  source: string;
  data: IdParsed;
};

export type ParseIdApiArg = {
  id?: string | null;
  inputText?: string | null;
  imageUrl?: string | null;
  image?: File | null;
};

export type IdParsed = {
  fullName: string;
  dateOfBirth: string;
  address: {
    street: string;
    city: string;
    state: string;
    country: string;
    zipCode: string;
  };
  documentNumber: string;
  expirationDate: string;
  issueDate: string;
  licenseClass: string;
  endorsements: string;
  restrictions: string;
  sex: string;
  eyeColor: string;
  height: string;
  detectedCountry: string;
  detectedState: string;
  barcodePresent: boolean;
  warnings: string[];
  confidences: Record<string, number>;
  boxes: Record<string, number[]>;
};

// Create a separate API for ID parsing with proper FormData handling
export const idParsingApi = createApi({
  reducerPath: 'idParsingApi',
  baseQuery: fetchBaseQuery({
    baseUrl: 'https://localhost:7583', // Use the direct API URL for now
  }),
  endpoints: (build) => ({
    parseId: build.mutation<ParseIdApiResponse, ParseIdApiArg>({
      query: (queryArg) => {
        const formData = new FormData();
        
        if (queryArg.id) formData.append('id', queryArg.id);
        if (queryArg.inputText) formData.append('inputText', queryArg.inputText);
        if (queryArg.imageUrl) formData.append('imageUrl', queryArg.imageUrl);
        if (queryArg.image) formData.append('image', queryArg.image);
        
        return {
          url: '/api/Parse/id',
          method: 'POST',
          body: formData,
        };
      },
    }),
  }),
});

// Export the hook
export const { useParseIdMutation } = idParsingApi;