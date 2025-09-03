/* eslint-disable */
import { emptySplitApi } from "./empty-api";

// Define types for the parsing API
export type ParseApiResponse = {
  source: string;
  data: Person;
};

export type ParseApiArg = {
  inputText?: string | null;
  id?: string | null;
};

export type Person = {
  name: string;
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
  phoneNumber: string;
};

// Create the parsing API by injecting endpoints into the base API
export const parsingApi = emptySplitApi.injectEndpoints({
  endpoints: (build) => ({
    parse: build.mutation<ParseApiResponse, ParseApiArg>({
      query: (queryArg) => ({
        url: `/api/Parse`,
        method: "POST",
        body: queryArg,
      }),
    }),
  }),
  overrideExisting: false,
});

// Export the hook
export const { useParseMutation } = parsingApi;