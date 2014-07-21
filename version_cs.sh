#!/bin/sh
function usage {
	echo "Usage $0 <options>"
	echo "-i PATH version number path [Required]"
	echo "-o PATH version header path [Required]"
	echo "-h print this message"
}
OPTSTRING="i:o:mMrh"

MAJOR_NAME=${MAJOR_NAME:-MAJOR_REVISION}
MINOR_NAME=${MINOR_NAME:-MINOR_REVISION}
BUILD_NAME=${BUILD_NAME:-BUILD_NUMBER}
REPO_NAME=${GIT_NAME:-REPO_REV}

INPUT_REV_PATH=
VERSION_PATH=
BUILD=0

while getopts ${OPTSTRING} c; do
	case ${c} in
    i) INPUT_REV_PATH="${OPTARG}";;
	o) VERSION_PATH="${OPTARG}";;
	h) usage ; exit 0;;
	\?) echo "Invalid option: -${OPTARG}" >&2; usage; exit 1 ;;
	:) echo "Option -${OPTARG} requires an argument." >&2; usage; exit 1 ;;
	*) echo "Unimplemented option: -${OPTARG}" >&2; usage; exit 1;;
	esac
done
shift $((OPTIND-1))

INPUT_REV_STR="$(cat "${INPUT_REV_PATH}" 2>/dev/null | sed 's/-.*//')"

if [ -z "${VERSION_PATH}" ]
then
	usage
	exit 1
fi
VERSION_PATH="$(readlink -f ${VERSION_PATH})"

if [ -f "${VERSION_PATH}" ]; then
    sed -i'' "/AssemblyVersion/ s/(\".*\")/(\"${INPUT_REV_STR}\")/" "${VERSION_PATH}"
else
    cat > "${VERSION_PATH}" <<EOF
using System;
using System.Reflection;

[assembly: AssemblyVersion("0.2-g08cb913")]
EOF
fi
