import { MAP_CREATION } from "../../shared/constants/Routes";
import ToggleButtonGroup from "@mui/material/ToggleButtonGroup";
import ToggleButton from "@mui/material/ToggleButton";
import { useState } from "react";
import { UserRole, useAuth } from "../../hooks/useAuth";
import { MapDto } from "../../shared/interfaces";
import { BasicTable, ContentWrapper, CustomLink } from "../../components";

const listVariants = [
  { name: "Published", queryValue: "true" },
  { name: "Unpublished", queryValue: "false" },
];

const baseEndpoint = "/maps";
const publishQueryKey = "?isPublished=";
function constructUrl(queryValue: string) {
  return `${baseEndpoint}${publishQueryKey}${queryValue}`;
}

const MapList = () => {
  const { userAuthData } = useAuth();

  const [selectedVariant, setSelectedVariant] = useState(0);
  const [endpoint, setEndpoint] = useState(
    constructUrl(listVariants[selectedVariant].queryValue),
  );

  function handleVariantChange(
    event: React.MouseEvent<HTMLElement>,
    newVariant: number | null,
  ) {
    event.preventDefault();
    if (newVariant !== null && selectedVariant !== newVariant) {
      setSelectedVariant(newVariant);
      setEndpoint(constructUrl(listVariants[newVariant].queryValue));
    }
  }

  function renderHeader(thClasses: string): React.ReactNode {
    return (
      <>
        <th className={thClasses}>Id</th>
        <th className={thClasses}>Name</th>
        <th className={thClasses}>Creator</th>
      </>
    );
  }

  function renderBody(map: MapDto, tdClasses: string): React.ReactNode {
    return (
      <>
        <td className={tdClasses}>{map.id}</td>
        <td className={tdClasses}>{map.name}</td>
        <td className={tdClasses}>{map.creator.userName}</td>
      </>
    );
  }

  function getDetailsRoute(map: MapDto): string {
    return `/users/${map.creator.id}/maps/${map.id}`;
  }

  return (
    <ContentWrapper title="Maps">
      {userAuthData?.roles.includes(UserRole.Admin) && (
        <ToggleButtonGroup
          color="primary"
          value={selectedVariant}
          exclusive
          onChange={handleVariantChange}
          aria-label="Platform"
        >
          {listVariants.map((v, idx) => (
            <ToggleButton key={idx} value={idx}>
              {v.name}
            </ToggleButton>
          ))}
        </ToggleButtonGroup>
      )}

      <BasicTable
        endpoint={endpoint}
        renderHeader={renderHeader}
        renderBody={renderBody}
        getDetailsRoute={getDetailsRoute}
      />

      <CustomLink to={MAP_CREATION} hasButtonStyle={true}>
        Create map
      </CustomLink>
    </ContentWrapper>
  );
};

export default MapList;
