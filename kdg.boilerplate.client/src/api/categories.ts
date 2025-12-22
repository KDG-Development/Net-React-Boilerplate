import { RequestMethodArgs } from "kdg-react"
import { MegaMenuCategories } from "../views/_common/templates/components/MegaMenu"

// Static placeholder data - replace with real API call when ready
const PLACEHOLDER_CATEGORIES: MegaMenuCategories = {
  office: {
    label: "Office",
    children: {
      "writing-supplies": { label: "Writing Supplies" },
      "desk-accessories": { label: "Desk Accessories" },
      "filing-storage": { label: "Filing & Storage" },
    },
  },
  technology: {
    label: "Technology",
    children: {
      computers: {
        label: "Computers",
        children: {
          desktops: { label: "Desktops" },
          laptops: { label: "Laptops" },
        },
      },
      peripherals: { label: "Peripherals" },
      software: { label: "Software" },
    },
  },
  furniture: {
    label: "Furniture",
    children: {
      desks: { label: "Desks" },
      chairs: { label: "Chairs" },
    },
  },
};

export const getCategories = async (args: RequestMethodArgs<MegaMenuCategories>) => {
  // TODO: Replace with real API call
  // await Api.Request.Get({
  //   url: `${Api.BASE_URL}/categories/mega-menu`,
  //   success: args.success,
  //   errorHandler: args.errorHandler,
  //   mapResult: (result: any): MegaMenuCategories => ({ ...result })
  // })

  // Simulate API delay
  await new Promise(resolve => setTimeout(resolve, 500));
  args.success(PLACEHOLDER_CATEGORIES);
}
