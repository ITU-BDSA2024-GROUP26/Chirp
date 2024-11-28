#Created with ChatGPT, with some modifications and additional comments for understanding

# Check if an argument is passed, and validate it
if [ -z "$1" ]; then # if no argument is passed
    pType="patch" # we default to incrementing the patch(last digit)
    echo "No argument provided, so defaulting to incrementing patch(v*.*.x)"
else
    pType="$1" #if we have an argument, then that's the patchtype we use
fi

# Fetch the latest tags from the remote
git fetch --tags

# Get the latest tag in the format vx.y.z
latest_tag=$(git describe --tags `git rev-list --tags --max-count=1`) 
# "git rev-list --tags --max-count=1" gives us the commit hash of the latest tag in our tree
# "git describe --tags" with the previous command passed in gives us the name of the tag associated with that commit

if [ -z "$latest_tag" ]; then
  # If no tags exist, start from v0.0.1
  new_tag="v0.0.1"
else
  # Extract the major, minor, and patch numbers from the latest tag
  # IFS is the Internal Field Seperator, which we set to seperate by '.'
  # read reads input from the string provided right of <<< 
  # -r makes it ignore backslashes and -a makes it throw the results into an array(called version_parts)
  # "${latest_tag//v/}", the //v/ globally removes all occurences of v(so getting rid of the v in v1.2.3)
  IFS='.' read -r -a version_parts <<< "${latest_tag//v/}"

  # then we just grab the numbers from the array into respective variables
  major="${version_parts[0]}"
  minor="${version_parts[1]}"
  patch="${version_parts[2]}"

   # Increment the correct version part based on the argument
  case "$pType" in
    major)
      major=$((major + 1))
      minor=0  # Reset minor and patch when incrementing major
      patch=0
      ;;
    minor)
      minor=$((minor + 1))
      patch=0  # Reset patch when incrementing minor
      ;;
    patch)
      patch=$((patch + 1))
      ;;
    *)
      echo "Error: Invalid argument. Use 'major', 'minor', 'patch' or provide no argument, in which case it defaults to patch."
      exit 1
      ;;
  esac

  # Create the new tag
  new_tag="v$major.$minor.$patch"
fi

# Create the new tag in git
git tag "$new_tag"

# Push the new tag(ahead of the commit, since github will associate the first commit after a tag is pushed with that tag)
git push origin "$new_tag"



echo "Created and pushed new tag: $new_tag"