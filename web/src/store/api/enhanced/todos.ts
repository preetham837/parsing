import { todosApi } from "../generated/todos";

export const enhancedTodosApi = todosApi.enhanceEndpoints({
    addTagTypes: [
        'TODO', 
    ],
    endpoints: {
        getTodos: {
            providesTags: ['TODO'],
        },
        createTodo: {
            invalidatesTags: ['TODO'],
        },
        updateTodo: {
            invalidatesTags: ['TODO'],
        },
        deleteTodo: {
            invalidatesTags: ['TODO'],
        },
    }
});

export const {
  useGetTodosQuery,
  useCreateTodoMutation,
  useUpdateTodoMutation,
  useDeleteTodoMutation,
} = enhancedTodosApi;