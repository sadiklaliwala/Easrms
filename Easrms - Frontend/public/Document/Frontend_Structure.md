src/
│
├── assets/
│   ├── Icons/
│   └── Images/
│
├── components/
│   │
│   ├── common/
│   │   │
│   │   ├── buttons/
│   │   │   ├── AppButton/
│   │   │   │   ├── AppButton.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppIconButton/
│   │   │   │   ├── AppIconButton.tsx
│   │   │   │   └── index.ts
│   │   │   └── AppLoadingButton/
│   │   │       ├── AppLoadingButton.tsx
│   │   │       └── index.ts
│   │   │
│   │   ├── dashboard/
│   │   │   ├── AppMetricCard/
│   │   │   │   ├── AppMetricCard.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppStatusChart/
│   │   │   │   ├── AppStatusChart.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppPriorityChart/
│   │   │   │   ├── AppPriorityChart.tsx
│   │   │   │   └── index.ts
│   │   │   └── AppCategoryChart/
│   │   │       ├── AppCategoryChart.tsx
│   │   │       └── index.ts
│   │   │
│   │   ├── feedback/
│   │   │   ├── AppToast/
│   │   │   │   ├── AppToast.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppLoader/
│   │   │   │   ├── AppLoader.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppSkeletonLoader/
│   │   │   │   ├── AppSkeletonLoader.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppEmptyState/
│   │   │   │   ├── AppEmptyState.tsx
│   │   │   │   └── index.ts
│   │   │   └── AppErrorState/
│   │   │       ├── AppErrorState.tsx
│   │   │       └── index.ts
│   │   │
│   │   ├── filter/
│   │   │   ├── AppSearchInput/
│   │   │   │   ├── AppSearchInput.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppFilterBar/
│   │   │   │   ├── AppFilterBar.tsx
│   │   │   │   └── index.ts
│   │   │   └── AppDateRangePicker/
│   │   │       ├── AppDateRangePicker.tsx
│   │   │       └── index.ts
│   │   │
│   │   ├── form/
│   │   │   ├── AppInput/
│   │   │   │   ├── AppInput.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppTextArea/
│   │   │   │   ├── AppTextArea.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppSelect/
│   │   │   │   ├── AppSelect.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppCheckbox/
│   │   │   │   ├── AppCheckbox.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppFormError/
│   │   │   │   ├── AppFormError.tsx
│   │   │   │   └── index.ts
│   │   │   └── AppLabel/
│   │   │       ├── AppLabel.tsx
│   │   │       └── index.ts
│   │   │
│   │   ├── layout/
│   │   │   ├── AppLayout/
│   │   │   │   ├── AppLayout.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppSidebar/
│   │   │   │   ├── AppSidebar.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppTopbar/
│   │   │   │   ├── AppTopbar.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppPageHeader/
│   │   │   │   ├── AppPageHeader.tsx
│   │   │   │   └── index.ts
│   │   │   └── AppCard/
│   │   │       ├── AppCard.tsx
│   │   │       └── index.ts
│   │   │
│   │   ├── modal/
│   │   │   ├── AppModal/
│   │   │   │   ├── AppModal.tsx
│   │   │   │   └── index.ts
│   │   │   ├── AppConfirmDialog/
│   │   │   │   ├── AppConfirmDialog.tsx
│   │   │   │   └── index.ts
│   │   │   └── AppDrawer/
│   │   │       ├── AppDrawer.tsx
│   │   │       └── index.ts
│   │   │
│   │   └── table/
│   │       ├── AppDataGrid/
│   │       │   ├── AppDataGrid.tsx
│   │       │   └── index.ts
│   │       ├── AppStatusBadge/
│   │       │   ├── AppStatusBadge.tsx
│   │       │   └── index.ts
│   │       ├── AppPriorityBadge/
│   │       │   ├── AppPriorityBadge.tsx
│   │       │   └── index.ts
│   │       ├── AppPagination/
│   │       │   ├── AppPagination.tsx
│   │       │   └── index.ts
│   │       └── AppTableActions/
│   │           ├── AppTableActions.tsx
│   │           └── index.ts
│   │
│   └── request/
│       ├── RequestStatusStepper/
│       │   ├── RequestStatusStepper.tsx
│       │   └── index.ts
│       ├── RequestCommentBox/
│       │   ├── RequestCommentBox.tsx
│       │   └── index.ts
│       ├── RequestHistoryTimeline/
│       │   ├── RequestHistoryTimeline.tsx
│       │   └── index.ts
│       ├── RequestActionButtons/
│       │   ├── RequestActionButtons.tsx
│       │   └── index.ts
│       └── RequestNumberBadge/
│           ├── RequestNumberBadge.tsx
│           └── index.ts
│
├── constants/
│   ├── priority.constants.ts
│   ├── role.constants.ts
│   └── status.constants.ts
│
├── hooks/
│   └── useAppSelector.ts           ← also exports useAppDispatch
│
├── pages/
│   ├── approval/
│   │   └── ApprovalQueuePage/
│   │       ├── ApprovalQueuePage.tsx
│   │       └── index.ts
│   ├── assignment/
│   │   └── AssignmentPage/
│   │       ├── AssignmentPage.tsx
│   │       └── index.ts
│   ├── auth/
│   │   └── LoginPage/
│   │       ├── LoginPage.tsx
│   │       └── index.ts
│   ├── categories/
│   │   └── CategoryPage/
│   │       ├── CategoryPage.tsx
│   │       └── index.ts
│   ├── dashboard/
│   │   └── DashboardPage/
│   │       ├── DashboardPage.tsx
│   │       └── index.ts
│   ├── requests/
│   │   ├── CreateRequestPage/
│   │   │   ├── CreateRequestPage.tsx
│   │   │   └── index.ts
│   │   ├── RequestDetailPage/
│   │   │   ├── RequestDetailPage.tsx
│   │   │   └── index.ts
│   │   └── RequestListPage/
│   │       ├── RequestListPage.tsx
│   │       └── index.ts
│   ├── support/
│   │   └── SupportTaskPage/
│   │       ├── SupportTaskPage.tsx
│   │       └── index.ts
│   └── users/
│       └── UserPage/
│           ├── UserPage.tsx
│           └── index.ts
│
├── routes/
│   ├── AppRoutes.tsx
│   ├── ProtectedRoute.tsx
│   └── RoleBasedRoute.tsx
│
├── store/
│   ├── ApiEndPoints.ts             ← centralized API URL constants
│   ├── index.ts                    ← configureStore, RootState, AppDispatch
│   ├── api/
│   │   ├── Api.ts                  ← createApi + baseQueryWithReauth
│   │   ├── baseQuery.ts
│   │   ├── auth.endpoints.ts
│   │   ├── category.endpoints.ts
│   │   ├── comment.endpoints.ts
│   │   ├── dashboard.endpoints.ts
│   │   ├── lookup.endpoints.ts
│   │   ├── request.endpoints.ts
│   │   └── user.endpoints.ts
│   └── slices/
│       └── authSlice.ts            ← setCredentials, clearCredentials, updateUser
│
├── types/
│   ├── auth.types.ts               ← LoginRequestDto, LoginResponseDto, CurrentUserDto, RefreshTokenRequestDto/ResponseDto
│   ├── category.types.ts           ← CategoryListDto, CategoryDetailDto, CreateCategoryDto, UpdateCategoryDto
│   ├── comment.types.ts            ← AddCommentDto, CommentListDto, StatusHistoryDto
│   ├── common.types.ts             ← ApiResponse<T>, PaginationDto, SupportUserLookupDto, ManagerLookupDto
│   ├── dashboard.types.ts          ← PriorityCountDto, CategoryCountDto, DashboardSummaryDto
│   ├── request.types.ts            ← RequestListDto, RequestDetailDto, CreateRequestDto, ApprovalRequestDto, AssignRequestDto, UpdateStatusDto, RequestListWithPaginationDto
│   └── user.types.ts               ← UserListDto, UserDetailDto, CreateUserDto, UpdateUserDto
│
├── utils/
│   ├── buildQueryParams.ts
│   ├── canPerformAction.ts
│   ├── formatDate.ts
│   ├── formatRequestNumber.ts
│   ├── getPriorityColor.ts
│   ├── getStatusColor.ts
│   └── hasRole.ts
│
├── App.tsx
├── main.tsx
└── vite-env.d.ts
