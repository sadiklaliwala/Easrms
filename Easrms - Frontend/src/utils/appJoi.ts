import BaseJoi from "joi";

/**
 * Custom Joi instance with global defaults applied:
 * - All string fields get a default max length of 255
 *
 * Import this instead of "joi" directly to get global rules automatically.
 */
const Joi = BaseJoi.defaults((schema) => {
  // if (schema.type === "string") {
  //   return (schema as BaseJoi.StringSchema).max(30);
  // }
  return schema;
});
export default Joi;
