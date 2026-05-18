// export const buildQueryParams = (
//   params: Record<string, string | number | boolean | undefined | null>
// ): string => {
//   const query = Object.entries(params)
//     .filter(([, value]) => value !== undefined && value !== null && value !== "")
//     .map(
//       ([key, value]) =>
//         `${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`
//     )
//     .join("&");

//   return query ? `?${query}` : "";
// };

export const buildQueryParams = <T extends object>(params: T): string => {
  const query = Object.entries(
    params as Record<string, string | number | boolean | undefined | null>,
  )
    .filter(
      ([, value]) => value !== undefined && value !== null && value !== "",
    )
    .map(
      ([key, value]) =>
        `${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`,
    )
    .join("&");

  return query ? `?${query}` : "";
};
